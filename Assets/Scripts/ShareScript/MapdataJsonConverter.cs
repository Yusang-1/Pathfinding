using UnityEngine;
using System.IO;

public class MapdataJsonConverter
{
    private readonly string personalMapFilePath;

    public MapdataJsonConverter()
    {
        personalMapFilePath = Application.persistentDataPath + "\\MapData";
    }

    public void SaveMapDataToJson(MapData mapData)
    {
        string json = JsonUtility.ToJson(mapData);
        if (!Directory.Exists(personalMapFilePath))
        {
            Directory.CreateDirectory(personalMapFilePath);
        }

        // Windows: C:\Users\<user>\AppData\LocalLow\<companyname>\<productname>
        var filePath = Path.Combine(personalMapFilePath, mapData.MapName + ".json");
        File.WriteAllText(filePath, json);

        PopupService.Show($"\'{mapData.MapName}\' is saved in \'{personalMapFilePath}\'");
    }

    public MapData[] GetPersonalSavedMaps()
    {
        if (!Directory.Exists(personalMapFilePath))
        {
            Directory.CreateDirectory(personalMapFilePath);
            return null;
        }

        string[] files = Directory.GetFiles(personalMapFilePath);
        var results = new MapData[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            results[i] = ConvertJsonToMapData(File.ReadAllText(files[i]));
        }

        return results;
    }
    public MapData[] GetOfficialSavedMaps()
    {
        var jsonFiles = Resources.LoadAll<TextAsset>("Maps");
        Debug.Log(jsonFiles.Length);
        var results = new MapData[jsonFiles.Length];
        for(int i = 0; i < jsonFiles.Length; i++)
        {
            results[i] = ConvertJsonToMapData(jsonFiles[i].text);
        }
        
        return results;
    }

    private MapData ConvertJsonToMapData(string json)
    {
        var mapData = JsonUtility.FromJson<MapData>(json);

        return mapData;
    }
}
