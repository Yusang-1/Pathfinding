using UnityEngine;
using System;

namespace Assets.Scripts.CreateMap.UI
{
    public class UIGenerateMap : MonoBehaviour
    {
        public event Action<int, int> OnGenerateMap;
        public event Action OnGenerateMapUI;
        public event Func<CreateMapManager.MapData[]> OnGetOfficialMapList;
        public event Func<CreateMapManager.MapData[]> OnGetPersonalMapList;
        public event Action<CreateMapManager.MapData> OnLoadMap;

        [SerializeField] private UIGenerateMapInput input;
        [SerializeField] private UILoadMapList loadMapList;
        [SerializeField] private GameObject generateUIs;

        private void Start()
        {
            OnGenerateMapUI += SetActiveFalse;
            loadMapList.OnLoadMapClosed += SetActiveGenerateUIs;
            loadMapList.OnLoadMap += (mapData) => OnLoadMap?.Invoke(mapData);
            loadMapList.OnLoadMapSuccessed += () => OnGenerateMapUI?.Invoke();
        }

        /// <summary> button에 할당 </summary>
        public void OnGenerateMapButton()
        {
            input.GetInput(out int mapSize, out int clusterSize);
            OnGenerateMap?.Invoke(mapSize, clusterSize);
            OnGenerateMapUI?.Invoke();
        }

        /// <summary> button에 할당 </summary>
        public void OnShowSavedMapButton()
        {
            var officlaMapList = OnGetOfficialMapList?.Invoke();
            var personalMapList = OnGetPersonalMapList?.Invoke();
            loadMapList.ShowMapList(officlaMapList, personalMapList);

            generateUIs.SetActive(false);
        }

        public void SetActiveTrue() => gameObject.SetActive(true);
        public void SetActiveFalse() => gameObject.SetActive(false);

        private void SetActiveGenerateUIs() => generateUIs.SetActive(true);
    }
}
