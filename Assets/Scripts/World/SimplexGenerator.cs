using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Permafrost.World
{
    #region Structs
    /// <summary>
    /// A simple container for both a noise layer's weight and scale values.
    /// Effectively a Vector2 in terms of data stored, but the context of being a noise
    /// layer (and isn't compatible with most Vector2 functions, not that you'd need
    /// them in context).
    /// </summary>
    [Serializable]
    public struct NoiseLayer
    {
        public float weight;
        public float scaleModifier;
        public FadeFunctionType fadeFunction;
    }

    /// <summary>
    /// The resulting data from the perlin noise functions.
    /// </summary>
    public struct PerlinResultData
    {
        public float[,] heights;
        public float forestationFactor;
        public float dryFactor;
    }

    /// <summary>
    /// Simple enum to determine what fade function this layer should use.
    /// </summary>
    public enum FadeFunctionType
    {
        Default, Peaks
    }
    #endregion

    /// <summary>
    /// TODO: biomes still need work?
    /// I almost wonder if I should attach biome data to a game object and
    /// then child it to the terrain? then when for example the feature generator
    /// goes to make trees n shit it can read biome data from it? heights are handled
    /// by the noise layer normally and the random float could be like forestation
    /// </summary>
    /// <remarks>
    /// Most of the simplex math is dont through async tasks so it can run in
    /// a background thread.
    /// Still a lot of thanks to https://adrianb.io/2014/08/09/perlinnoise.html, 
    /// this code is fairly modified by this point but was originally based on it.
    /// </remarks>
    public class SimplexGenerator : MonoBehaviour
    {
        #region Data
        private byte[] heightPermutation;
        private byte[] heights;
        private float3[] biomePermutation;
        private float3[] biomes;

        [Header("Generation Settings")]
        [SerializeField] private int permutationTableSize = 256;
        [SerializeField] private NoiseLayer[] noiseLayers;
        [SerializeField] private NoiseLayer biomeNoiseLayer;

        [Header("Component References")]
        [SerializeField] private MasterTerrainGenerator masterGenerator;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;
        #endregion

        #region Permutation
        /// <summary>
        /// Generates new permutation tables to use for noise generation.
        /// </summary>
        /// <returns>True if completed successfully, false otherwise. Really not sure if I need to do this but it helps with error reporting.</returns>
        public bool GeneratePermutations()
        {
            // I guess these are just permutation tables not really noise
            // maps but...
            heightPermutation = new byte[permutationTableSize];
            heights = new byte[permutationTableSize * 2];
            biomePermutation = new float3[permutationTableSize];
            biomes = new float3[permutationTableSize * 2];

            if (masterGenerator.MasterRNG == null)
            {
                if (debugEnabled) Debug.LogError("[SimplexGenerator] No master terrain RNG present!");
                return false;
            }

            // heights
            int i;
            for (i = 0; i < permutationTableSize; i++) heightPermutation[i] = (byte) masterGenerator.MasterRNG.Next(0, 255);
            for (i = 0; i < permutationTableSize; i++) heights[i] = heightPermutation[i];
            for (; i < permutationTableSize * 2; i++) heights[i] = heights[i - permutationTableSize]; // technically a microoptimization over using modulus

            // biomes
            for (i = 0; i < permutationTableSize; i++) biomePermutation[i] = new((byte) masterGenerator.MasterRNG.Next(0, 255), (float) masterGenerator.MasterRNG.NextDouble(), (float) masterGenerator.MasterRNG.NextDouble());
            for (i = 0; i < permutationTableSize; i++) biomes[i] = biomePermutation[i];
            for (; i < permutationTableSize * 2; i++) biomes[i] = biomes[i - permutationTableSize];

            return true;
        }

        /// <summary>
        /// Returns a copy of the heightmap permutation array.
        /// </summary>
        /// <returns></returns>
        public byte[] GetHeightmapPermutation()
        {
            byte[] temp = new byte[permutationTableSize];
            Array.Copy(heightPermutation, temp, permutationTableSize);
            return temp;
        }

        /// <summary>
        /// Returns a copy of the biomemap permutation array.
        /// </summary>
        /// <returns></returns>
        public float3[] GetBiomemapPermutation()
        {
            float3[] temp = new float3[permutationTableSize];
            Array.Copy(biomePermutation, temp, permutationTableSize);
            return temp;
        }
        #endregion

        #region Perlin/Simplex Noise
        /// <summary>
        /// Crunches the math for determining the height of every point in the terrain cell.
        /// This function can be called asynchronously. In the traditional, C# task sense though.
        /// Not a coroutine, although you could plug this into a coroutine by using WaitUntil I believe.
        /// </summary>
        /// <param name="cellSize">The terrain cell size.</param>
        /// <param name="cellX">The cell's X position relative to other cells. Example: If this is the 5th cell left, this would be 4 due to zero indexing.</param>
        /// <param name="cellY">The cell's Y position relative to other cells. Example: If this is the -3rd cell up, this would be -3 due to zero indexing.</param>
        /// <param name="cancel">A cancellation token in case the operation has to be cancelled midway through generation.</param>
        /// <returns>A Task that upon completion returns a 2D array of floats representing the height values of the grid chunk.</returns>
        public async Task<PerlinResultData> GenerateCellHeights(int cellSize, int cellX, int cellY, CancellationToken cancel)
        {
            return await Task.Run(() => {
                float[,] heights = new float[cellSize + 1, cellSize + 1];
                float forestation = 0;
                float dry = 0;
                int l = 0;
                if (debugEnabled)
                {
                    Debug.Log($"[SimplexGenerator] cellX: {cellX}, cellY: {cellY}");
                }

                for (int i = 0; i < cellSize + 1; i++)
                {
                    if (cancel.IsCancellationRequested) break;
                    for (int j = 0; j < cellSize + 1; j++)
                    {
                        heights[i, j] = 0;
                        for (int n = 0; n < noiseLayers.Length; n++)
                        {
                            heights[i, j] += noiseLayers[n].weight * Perlin(
                                cellX * noiseLayers[n].scaleModifier + (j / (float) cellSize * noiseLayers[n].scaleModifier),
                                cellY * noiseLayers[n].scaleModifier + (i / (float) cellSize * noiseLayers[n].scaleModifier),
                                noiseLayers[n].fadeFunction);
                        }
                        heights[i, j] += biomeNoiseLayer.weight * BiomePerlin(
                            cellX * biomeNoiseLayer.scaleModifier + (j / (float)cellSize * biomeNoiseLayer.scaleModifier),
                            cellY * biomeNoiseLayer.scaleModifier + (i / (float)cellSize * biomeNoiseLayer.scaleModifier),
                            ref forestation, ref dry, biomeNoiseLayer.fadeFunction);
                        l++;
                    }
                }
                PerlinResultData data = new()
                {
                    heights = heights,
                    forestationFactor = forestation / (float)l,
                    dryFactor = dry / (float)l
                };
                return data;
            }, cancel);
        }

        /// <summary>
        /// I hate default modulus implementations so god damn much.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        private int PositiveMod(int n, int m)
        {
            return ((n % m) + n) % m;
        }

        /// <summary>
        /// Individual math for finding the height of any terrain point.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <returns>The height of the specified point.</returns>
        private float Perlin(float x, float y, FadeFunctionType t)
        {
            // Separate int and float portions of the coords
            int xi = (int) x, yi = (int) y;
            float xf = x - xi, yf = y - yi;

            // Fade function coefficients?
            float u = 0, v = 0;
            switch (t)
            {
                case FadeFunctionType.Default:
                    u = PerlinFade(xf);
                    v = PerlinFade(yf);
                    break;
                case FadeFunctionType.Peaks:
                    u = PerlinFadePeaks(xf);
                    v = PerlinFadePeaks(yf);
                    break;
            }

            // Get the gradient vector offsets via this silly hash function magic
            byte aa, ab, ba, bb;
            //xi = PositiveMod(xi, heights.Length);
            //yi = PositiveMod(yi, heights.Length);
            aa = heights[PositiveMod(heights[xi] + yi, heights.Length)];
            ab = heights[PositiveMod(heights[xi] + yi + 1, heights.Length)];
            ba = heights[PositiveMod(heights[xi + 1] + yi, heights.Length)];
            bb = heights[PositiveMod(heights[xi + 1] + yi + 1, heights.Length)];

            // Lerp it all together
            float lerp1, lerp2;
            lerp1 = math.lerp(PerlinGradient(aa, xf, yf, 0), PerlinGradient(ba, xf - 1, yf, 0), u);
            lerp2 = math.lerp(PerlinGradient(ab, xf, yf - 1, 0), PerlinGradient(bb, xf - 1, yf - 1, 0), u);
            return (math.lerp(lerp1, lerp2, v) + 1) / 2;
        }

        /// <summary>
        /// Basically a lookup table that lets you find the proper gradient vectors for
        /// the specified point.
        /// </summary>
        /// <param name="hash">A number, 0 to 15, used as an index.</param>
        /// <param name="x">The float component of the x position.</param>
        /// <param name="y">The float component of the y position.</param>
        /// <param name="z">Unused due to terrain not having caves and shit.</param>
        /// <returns>A float that applies the gradient vector to the float components.</returns>
        private float PerlinGradient(int hash, float x, float y, float z)
        {
            return (hash & 0xF) switch
            {
                0x0 => x + y,
                0x1 => -x + y,
                0x2 => x - y,
                0x3 => -x - y,
                0x4 => x + z,
                0x5 => -x + z,
                0x6 => x - z,
                0x7 => -x - z,
                0x8 => y + z,
                0x9 => -y + z,
                0xA => y - z,
                0xB => -y - z,
                0xC => y + x,
                0xD => -y + z,
                0xE => y - x,
                0xF => -y - z,
                _ => 0,// never happens
            };
        }

        /// <summary>
        /// The fade function that smooths out the noise.
        /// </summary>
        /// <param name="val">An x or y float position.</param>
        /// <returns></returns>
        private float PerlinFade(float val)
        {
            return val * val * val * (val * (val * 6 - 15) + 10);
        }

        /// <summary>
        /// A second fade function that creates tall peaks at its highest.
        /// </summary>
        /// <param name="val">An x or y float position.</param>
        /// <returns></returns>
        private float PerlinFadePeaks(float val)
        {
            return val * val * val * (val * (val * val * 6 - 15) + 10);
        }

        /// <summary>
        /// A slight variation of the standard Perlin function for the Biomes
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="d">The dry factor of the chunk's biome.</param>
        /// <param name="f">The forestation factor of the chunk's biome.</param>
        /// <returns></returns>
        private float BiomePerlin(float x, float y, ref float f, ref float d, FadeFunctionType t)
        {
            // Separate int and float portions of the coords
            int xi = (int) x, yi = (int) y;
            float xf = x - xi, yf = y - yi;

            // Fade function coefficients?
            float u = 0, v = 0;
            switch (t)
            {
                case FadeFunctionType.Default:
                    u = PerlinFade(xf);
                    v = PerlinFade(yf);
                    break;
                case FadeFunctionType.Peaks:
                    u = PerlinFadePeaks(xf);
                    v = PerlinFadePeaks(yf);
                    break;
            }

            // Get the gradient vector offsets via this silly hash function magic
            // This is where things are gonna start changing here
            byte aa, ab, ba, bb;
            aa = (byte) biomes[PositiveMod((int) biomes[xi].x + yi, heights.Length)].x;
            ab = (byte) biomes[PositiveMod((int) biomes[xi].x + yi + 1, heights.Length)].x;
            ba = (byte) biomes[PositiveMod((int) biomes[xi + 1].x + yi, heights.Length)].x;
            bb = (byte) biomes[PositiveMod((int) biomes[xi + 1].x + yi + 1, heights.Length)].x;

            // Biome height modifier? Do I want that?
            /*float xx, xy, yx, yy;
            xx = biomes[PositiveMod((int) biomes[xi].x + yi, heights.Length)].y;
            xy = biomes[PositiveMod((int) biomes[xi].x + yi + 1, heights.Length)].y;
            yx = biomes[PositiveMod((int) biomes[xi + 1].x + yi, heights.Length)].y;
            yy = biomes[PositiveMod((int) biomes[xi + 1].x + yi + 1, heights.Length)].y;*/

            // Lerp it all together
            float lerp1, lerp2, fx, fy;
            lerp1 = math.lerp(PerlinGradient(aa, xf, yf, 0), PerlinGradient(ba, xf - 1, yf, 0), u);
            lerp2 = math.lerp(PerlinGradient(ab, xf, yf - 1, 0), PerlinGradient(bb, xf - 1, yf - 1, 0), u);
            fx = math.lerp(biomes[xi].y, biomes[PositiveMod(xi + 1, biomes.Length)].y, xf);
            fy = math.lerp(biomes[yi].y, biomes[PositiveMod(yi + 1, biomes.Length)].y, yf);
            f += (fx + fy) / 2;
            fx = math.lerp(biomes[xi].z, biomes[PositiveMod(xi + 1, biomes.Length)].z, xf);
            fy = math.lerp(biomes[yi].z, biomes[PositiveMod(yi + 1, biomes.Length)].z, yf);
            d += (fx + fy) / 2;

            return ((math.lerp(lerp1, lerp2, v) + 1) / 2);
        }
        #endregion
    }
}
// 83 SLOC