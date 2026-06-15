using UnityEngine;
using System;

public class UIGenerateMap : MonoBehaviour
{
    /// <summary> 맵 생성의 구현 코드들 </summary>
        public event Action<int, int> OnGenerateMapRequested;

        /// <summary> 맵이 생성되었을 때 UI의 작동 </summary>
        public event Action OnGenerateMapUI;
        private Func<int> OnMapSizeRequested;
        private Func<int> OnClusterSizeRequested;

        public event Func<MapData[]> OnOfficialMapListRequested;
        public event Func<MapData[]> OnPersonalMapListRequested;
        private Action<MapData[], MapData[]> ShowSavedMapsAction;


        private void Start()
        {
            OnGenerateMapUI += SetActiveFalse;
        }

        public void SetProviders(Func<int> mapSizeProvider, Func<int> clusterSizeProvider,
                                Action<MapData[], MapData[]> showSavedMaps)
        {
            OnMapSizeRequested = mapSizeProvider;
            OnClusterSizeRequested = clusterSizeProvider;
            ShowSavedMapsAction = showSavedMaps;
        }

        /// <summary> button에 할당 </summary>
        public void OnGenerateMapButton()
        {
            int mapSize = OnMapSizeRequested?.Invoke() ?? 0;
            int clusterSize = OnClusterSizeRequested?.Invoke() ?? 0;

            OnGenerateMapRequested?.Invoke(mapSize, clusterSize);
            OnGenerateMapUI?.Invoke();
        }

        /// <summary> button에 할당 </summary>
        public void OnShowSavedMapButton()
        {
            var officlaMapList = OnOfficialMapListRequested?.Invoke();
            var personalMapList = OnPersonalMapListRequested?.Invoke();
            ShowSavedMapsAction?.Invoke(officlaMapList, personalMapList);

            gameObject.SetActive(false);
        }

        public void SetActiveTrue() => gameObject.SetActive(true);
        public void SetActiveFalse() => gameObject.SetActive(false);
}
