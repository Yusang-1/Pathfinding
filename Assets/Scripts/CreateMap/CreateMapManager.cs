using UnityEngine;
using Assets.Scripts.CreateMap.UI;
using Assets.Scripts.ControllUnit;
using System.Collections.Generic;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.CreateMap
{
    // createMap 씬의 Manager
    // 얘의 역할을 줄이자, 원래 manager역할은 mapManager를 사용하고 맵 저장 기능만 사용할 수 있게
    public class CreateMapManager : MonoBehaviour
    {
        [SerializeField] private Node nodePrefab;
        [SerializeField] private NodeData nodeData;
        [SerializeField] private CreateMapUIRoot uiRoot;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private UnitSpawner unitSpawner;
        [SerializeField] private UnitsSO unitsSO;

        private NodeList nodeList;
        private MapGenerator mapGenerator;
        private MapdataJsonConverter mapdataJsonConverter;
        private readonly SpatialHash spatialHash = new();
        private MapRuntimeContext mapRuntimeContext;
        private UnitRuntimeContext unitRuntimeContext;
        private UnitSpawnHolder unitSpawnHolder;

        [SerializeField] private int nodeSize;
        private int mapSize;

        private void Start()
        {
            unitRuntimeContext = new(null, spatialHash);
            
            nodeData.Initialize();
            unitSpawner.Initialize(unitRuntimeContext);
            unitsSO.Initialize();

            nodeList = new NodeList(nodeData);
            nodeList.OnSelected += nodeList.NodeTypeController.SetNodeType;

            mapRuntimeContext = new MapRuntimeContext(null, nodeData);
            mapdataJsonConverter = new MapdataJsonConverter(mapRuntimeContext.LoadedMapData);
            mapGenerator = new MapGenerator(nodePrefab, nodeList, unitSpawner);
            unitSpawnHolder = new UnitSpawnHolder(unitSpawner);

            uiRoot.OnGenerateMapRequested += CreateEmptyMap;
            uiRoot.OnTileSelectorRequested += nodeList.NodeTypeController.SetCurrentSelected;
            uiRoot.OnExportMapRequested += ExportMap;
            uiRoot.OnClearMapRequested += nodeList.NodeTypeController.NodeTypeDrawer.ResetAllNodes;
            uiRoot.OnRemoveMapRequested += nodeList.NodeTypeController.NodeTypeDrawer.ResetAllNodes;
            uiRoot.OnRemoveMapRequested += nodeList.DestroyNodes;
            uiRoot.OnGetPersonalMapListRequested += mapdataJsonConverter.GetPersonalSavedMaps;
            uiRoot.OnGetOfficialMapListRequested += mapdataJsonConverter.GetOfficialSavedMaps;
            uiRoot.OnLoadMapRequested += LoadSavedMap;
            uiRoot.Initialize();
            
            inputManager.OnControllMenu += () => uiRoot.OnControllMenu?.Invoke();

            uiRoot.OnSpawnEvent += unitSpawner.StartSetSpawnArea;
            uiRoot.OnSpawnUnitCode += unitSpawnHolder.ReserveSpawnUnitCode;
            unitSpawner.OnSpawnAreaSettingStarted += inputManager.ChangeActionMapSelected;
            inputManager.OnSpawnUnitRequested += unitSpawnHolder.Spawn;
            inputManager.OnSetSpawnAreaFinished += unitSpawner.FinishSetSpawnArea;
        }

        private void CreateEmptyMap(int sizeOfMap, int sizeOfCluster)
        {
            const int defaultMapSize = 20;

            if (sizeOfMap == 0)
            {
                mapSize = defaultMapSize;
            }
            else
            {
                mapSize = sizeOfMap;
            }

            mapGenerator.GenerateMap(mapSize);
        }

        public void LoadSavedMap(int mapCode)
        {
            if (!mapRuntimeContext.LoadedMapData.TryGetMapData(mapCode, out MapData mapData))
            {
                return;
            }
            mapSize = mapData.InfoData.MapSize;
            
            nodeList.Initialize(MapRuntimeContext.NODE_SIZE, mapSize);
            mapGenerator.GenerateMap(mapSize, mapData.TerrainData);
            mapGenerator.SetUnit(mapData.UnitData);
        }

        public void ExportMap(string mapName)
        {
            Vector2Int[] obstacleIndexes = nodeList.NodeTypeController.NodeTypeDrawer.GetNodeInfo()[NodeType.obstacle].ToArray();

            MapData.Info infoData = new()
            {
                MapCode = mapdataJsonConverter.GetPersonalMapCode(),
                MapName = mapName,
                MapSize = mapSize
            };

            MapData.Terrain terrainData = new()
            {
                MapSize = mapSize,
                ObstacleIndexes = obstacleIndexes
            };

            MapData.Unit unitData;
            if (spatialHash.TryGetAllUnits(out List<int> units, out List<Vector3> positions))
            {
                float[] x = new float[positions.Count];
                float[] z = new float[positions.Count];
                float[] y = new float[positions.Count];
                
                for(int index = 0; index < positions.Count; index++)
                {
                    x[index] = positions[index].x;
                    y[index] = positions[index].y;
                    z[index] = positions[index].z;
                }
                
                unitData = new()
                {
                    UnitCodes = units.ToArray(),
                    PosX = x,
                    PosY = y,
                    PosZ = z
                };
            }
            else
            {
                unitData = default;
            }

            var mapData = new MapData(infoData, terrainData, unitData);

            mapdataJsonConverter.SaveMapDataToJson(mapData);
        }        
    }
}
