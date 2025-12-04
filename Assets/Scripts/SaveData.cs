using System.IO;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    // Data to save
    public float timeSurvived;
    public int killCount;
    public int itemsGathered;
    public int firesBuilt;
    public int id;

    private DataProcessing dataFile;
    private string path;
    private float autosaveTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        path = Application.persistentDataPath + "/" + System.DateTime.UtcNow.ToString("yy-MM-dd_hh-mm-ss") + ".json";
        Debug.Log("Saving data for this run to " + path);
    }

    // Update is called once per frame
    void Update()
    {
        autosaveTimer += Time.deltaTime;
        if (autosaveTimer > 1)
        {
            CreateDataToSave();
        }
    }

    private void OnApplicationQuit()
    {
        CreateDataToSave();
    }

    public void CreateDataToSave()
    {
        dataFile = new DataProcessing(timeSurvived, killCount, itemsGathered, firesBuilt);
        //Debug.Log(timeSurvived + " " + killCount + " " + itemsGathered + " " + firesBuilt);
        Save();
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(dataFile);
        StreamWriter writer = new StreamWriter(path);
        writer.Write(json);
        writer.Close();
    }
}
