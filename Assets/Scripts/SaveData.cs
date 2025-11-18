using System.IO;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    // Data to save
    public float timeSurvived;
    public int id;

    private DataProcessing dataFile;
    private string path;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        path = Application.persistentDataPath + "/" + System.DateTime.UtcNow.ToString() + ".json";
        Debug.Log("Saving data for this run to " + path);
    }

    // Update is called once per frame
    void Update()
    {
        // todo!!!!!!
    }

    public void CreateDataToSave()
    {
        dataFile = new DataProcessing(timeSurvived);
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
