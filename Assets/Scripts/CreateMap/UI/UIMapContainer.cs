using UnityEngine;
using System;
using TMPro;

namespace Assets.Scripts.CreateMap.UI
{
    public class UIMapContainer : MonoBehaviour
    {
        private Action<MapData.Info> onSelectMap;

        [SerializeField] private TextMeshProUGUI mapNameText;

        private MapData.Info mapInfoData;

        public void Initialize(MapData.Info mapInfoData, Action<MapData.Info> setUIInfo)
        {
            onSelectMap ??= setUIInfo;

            this.mapInfoData = mapInfoData;
            mapNameText.text = mapInfoData.MapName;

            gameObject.SetActive(true);
        }

        /// <summary> button에 할당 </summary>        
        public void OnSelect()
        {
            onSelectMap?.Invoke(mapInfoData);
        }
    }
}
