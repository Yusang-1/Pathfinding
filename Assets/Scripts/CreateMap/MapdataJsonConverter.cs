using UnityEngine;
using System.IO;

namespace Assets.Scripts.CreateMap
{
    public class MapdataJsonConverter
    {
        public void SaveMapDataToJson(CreateMapManager.MapData mapData)
        {
            string json = JsonUtility.ToJson(mapData);
            
            // Windows: C:\Users\<user>\AppData\LocalLow\<companyname>\<productname>
            var filePath = Path.Combine(Application.persistentDataPath, mapData.MapName + ".json");
            File.WriteAllText(filePath, json);
            
            PopupService.Show($"\'{mapData.MapName}\' is saved in \'{Application.persistentDataPath}\'");
        }
        
        public CreateMapManager.MapData ConvertJsonToMapData(string json)
        {
            var mapData = JsonUtility.FromJson<CreateMapManager.MapData>(json);
            
            return mapData;
        }
   }
}
