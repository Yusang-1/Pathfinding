using UnityEngine;
using System;
using TMPro;

namespace Assets.Scripts.CreateMap.UI
{
    public class UIMapContainer : MonoBehaviour
    {
        private Action<CreateMapManager.MapData> onSelectMap;

        [SerializeField] private TextMeshProUGUI mapNameText;

        private CreateMapManager.MapData mapData;

        public void Initialize(CreateMapManager.MapData data, Action<CreateMapManager.MapData> setUIInfo)
        {
            onSelectMap ??= setUIInfo;

            mapData = data;
            mapNameText.text = data.MapName;

            gameObject.SetActive(true);
        }

        /// <summary> button에 할당 </summary>        
        public void OnSelect()
        {
            onSelectMap?.Invoke(mapData);
        }
    }
}
