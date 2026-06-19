using UnityEngine;
using System;

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
        public event Action<MapData> OnLoadMapRequested;
        public Action OnControllMenu;

        [SerializeField] private UIGenerateMapMediator uiGenerateMapMediator;
        [SerializeField] private UIModifyMapMediator uiModifyMapMediator;
        [SerializeField] private UIContainerScenes uiContainerScenes;
        [SerializeField] private UIPopup uiPopup;

        public void Initialize()
        {
            uiPopup.Initialize();
            uiGenerateMapMediator.Initialize();
            uiModifyMapMediator.Initialize();

            uiGenerateMapMediator.OnGenerateMapRequested += (mapSize, clusterSize) => OnGenerateMapRequested?.Invoke(mapSize, clusterSize);

            uiGenerateMapMediator.OnGenerateMapUI += uiModifyMapMediator.SetActiveTrue;
            uiGenerateMapMediator.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);

            uiGenerateMapMediator.OnOfficialMapListRequested += () => OnGetOfficialMapListRequested?.Invoke();
            uiGenerateMapMediator.OnPersonalMapListRequested += () => OnGetPersonalMapListRequested?.Invoke();

            uiModifyMapMediator.OnTileSelectorRequested += (nodeType) => OnTileSelectorRequested?.Invoke(nodeType);
            uiModifyMapMediator.OnExportMapRequested += (map) => OnExportMapRequested?.Invoke(map);
            uiModifyMapMediator.OnClearMapRequested += () => OnClearMapRequested?.Invoke();
            uiModifyMapMediator.OnRemoveMapRequested += () => OnRemoveMapRequested?.Invoke();
            uiModifyMapMediator.OnRemoveMapRequested += uiGenerateMapMediator.SetActiveTrue;
            
            OnControllMenu += uiContainerScenes.OnControllMenu;
        }
    }
}
