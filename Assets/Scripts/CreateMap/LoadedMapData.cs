using System.Collections.Generic;

namespace Assets.Scripts.CreateMap
{
    public class LoadedMapData
    {
        private readonly Dictionary<int, MapData> mapDataDict = new();

        public void SetOfficialMapDatas(MapData[] datas)
        {
            for (int index = 0; index < datas.Length; index++)
            {
                var info = datas[index].InfoData;

                if (mapDataDict.ContainsKey(info.MapCode))
                {
                    mapDataDict.Remove(info.MapCode);
                }

                mapDataDict.Add(info.MapCode, datas[index]);
            }
        }

        public void SetPersonalMapDatas(MapData[] datas)
        {
            for (int index = 0; index < datas.Length; index++)
            {
                var info = datas[index].InfoData;

                if (mapDataDict.ContainsKey(info.MapCode))
                {
                    mapDataDict.Remove(info.MapCode);
                }

                mapDataDict.Add(info.MapCode, datas[index]);
            }
        }

        public bool TryGetMapData(int mapCode, out MapData mapData)
        {
            if (!mapDataDict.ContainsKey(mapCode))
            {
                mapData = null;
                return false;
            }

            mapData = mapDataDict[mapCode];
            return true;
        }
    }
}
