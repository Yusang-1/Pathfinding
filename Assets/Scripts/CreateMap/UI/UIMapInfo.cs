using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.CreateMap.UI
{
    public class UIMapInfo : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI mapName;

        public void SetInfo(MapData mapData)
        {
            mapName.text = mapData.MapName;
        }
    }
}

