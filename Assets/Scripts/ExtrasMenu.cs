using UnityEngine;

public class ExtrasMenu : MonoBehaviour
{
    [SerializeField] private Mesh[] allTheModels;
    [SerializeField] private Material[] allTheMaterials;
    [SerializeField] private Vector3[] allTheScales;
    [SerializeField] private MeshFilter filter;
    [SerializeField] private MeshRenderer render;
    [SerializeField] private int modelIndex;
    [SerializeField] private bool textured;
    [SerializeField] private float rotateSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        render.transform.Rotate(Time.deltaTime * rotateSpeed * Vector3.up);
    }

    private void DisplayModel()
    {
        if (modelIndex < 0) return;
        filter.mesh = allTheModels[modelIndex];
        if (textured) render.material = allTheMaterials[modelIndex];
        else render.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        render.transform.localScale = allTheScales[modelIndex];
    }

    public void SwitchToModel(int index)
    {
        modelIndex = index;
        DisplayModel();
    }
}
