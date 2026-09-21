using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.CreateMap
{
    public class MapdataJsonConverter
    {
        // Windows: C:\Users\<user>\AppData\LocalLow\<companyname>\<productname>
        private readonly string personalMapFilePath;

        private readonly LoadedMapData loadedMapData;

        public MapdataJsonConverter(LoadedMapData loadedMapData)
        {
            personalMapFilePath = Application.persistentDataPath + "\\MapData";
            this.loadedMapData = loadedMapData;
        }

        public void SaveMapDataToJson(MapData mapData)
        {
            string json = JsonConvert.SerializeObject(mapData, Formatting.Indented);

            if (!Directory.Exists(personalMapFilePath))
            {
                Directory.CreateDirectory(personalMapFilePath);
            }

            string filePath = Path.Combine(personalMapFilePath, $"{mapData.InfoData.MapName}.json");
            File.WriteAllText(filePath, json);

            PopupService.Show($"\'{mapData.InfoData.MapName}\' is saved in \'{personalMapFilePath}\'");
        }

        public MapData[] GetPersonalSavedMaps()
        {
            if (!Directory.Exists(personalMapFilePath))
            {
                Directory.CreateDirectory(personalMapFilePath);

                return null;
            }

            string[] files = Directory.GetFiles(personalMapFilePath);

            var mapDatas = new MapData[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                mapDatas[i] = ConvertJsonToMapData(File.ReadAllText(files[i]));
            }

            loadedMapData.SetPersonalMapDatas(mapDatas);

            return mapDatas;
        }

        public MapData[] GetOfficialSavedMaps()
        {
            var jsonFiles = Resources.LoadAll<TextAsset>("Maps");

            var results = new MapData[jsonFiles.Length];
            for (int i = 0; i < jsonFiles.Length; i++)
            {
                results[i] = ConvertJsonToMapData(jsonFiles[i].text);
            }

            loadedMapData.SetOfficialMapDatas(results);

            return results;
        }

        private MapData ConvertJsonToMapData(string json)
        {
            JToken root = JToken.Parse(json);
            
            var info = JsonConvert.DeserializeObject<MapData.Info>(root["InfoData"].ToString());
            var terrain = JsonConvert.DeserializeObject<MapData.Terrain>(root["TerrainData"].ToString());
            var unit = JsonConvert.DeserializeObject<MapData.Unit>(root["UnitData"].ToString());
            
            var mapData = new MapData(info, terrain, unit);

            return mapData;
        }

        public int GetPersonalMapCode()
        {
            if (!Directory.Exists(personalMapFilePath))
            {
                Directory.CreateDirectory(personalMapFilePath);

                return 0;
            }

            return Directory.GetFiles(personalMapFilePath).Length;
        }
    }
}
