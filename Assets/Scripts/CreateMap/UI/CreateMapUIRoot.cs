using UnityEngine;
using System;
using Assets.Scripts.ControllUnit.UI;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.CreateMap.UI
{
    public class CreateMapUIRoot : MonoBehaviour
    {
        public event Action<int, int> OnGenerateMapRequested;
        public event Action<NodeType> OnTileSelectorRequested;
        public event Action<string> OnExportMapRequested;
        public event Action OnClearMapRequested;
        public event Action OnRemoveMapRequested;
        public event Func<MapData[]> OnGetOfficialMapListRequested;
        public event Func<MapData[]> OnGetPersonalMapListRequested;
        public event Action<int> OnLoadMapRequested;
        public Action OnControllMenu;


        [SerializeField] private UIGenerateMapMediator uiGenerateMapMediator;
        [SerializeField] private UIModifyMapMediator uiModifyMapMediator;
        [SerializeField] private UIContainerScenes uiContainerScenes;
        [SerializeField] private UIPopup uiPopup;
        [SerializeField] private UISpawnUnit uiSpawnUnit;

        private UnitSpawner unitSpawner;

        public void Initialize(AbstractSpawner spawner)
        {
            unitSpawner = spawner as UnitSpawner;

            uiPopup.Initialize();
            uiGenerateMapMediator.Initialize();
            uiModifyMapMediator.Initialize();

            uiGenerateMapMediator.OnGenerateMapRequested += (mapSize, clusterSize) => OnGenerateMapRequested?.Invoke(mapSize, clusterSize);

            uiGenerateMapMediator.OnGenerateMapUI += uiModifyMapMediator.SetActiveTrue;
            uiGenerateMapMediator.OnGenerateMapUI += uiSpawnUnit.SetActiveTrue;
            uiGenerateMapMediator.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);

            uiGenerateMapMediator.OnOfficialMapListRequested += () => OnGetOfficialMapListRequested?.Invoke();
            uiGenerateMapMediator.OnPersonalMapListRequested += () => OnGetPersonalMapListRequested?.Invoke();

            uiModifyMapMediator.OnTileSelectorRequested += (nodeType) => OnTileSelectorRequested?.Invoke(nodeType);
            uiModifyMapMediator.OnExportMapRequested += (map) => OnExportMapRequested?.Invoke(map);
            uiModifyMapMediator.OnClearMapRequested += () => OnClearMapRequested?.Invoke();
            uiModifyMapMediator.OnRemoveMapRequested += () => OnRemoveMapRequested?.Invoke();
            uiModifyMapMediator.OnRemoveMapRequested += uiGenerateMapMediator.SetActiveTrue;
            uiModifyMapMediator.OnRemoveMapRequested += uiSpawnUnit.SetActiveFalse;
            
            OnControllMenu += uiContainerScenes.OnControllMenu;

            uiSpawnUnit.OnSpawnUnitRequested += unitSpawner.SpawnUnit;
            // uiSpawnUnit.OnGetSpawnAreaRequested += ;
            // uiSpawnUnit.OnGetSpawnAreaFinished += ;
        }
    }
}
