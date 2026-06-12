using System;
using UnityEngine;

namespace Assets.Scripts.CreateMap.UI
{
    public class CreateMapUIRoot : MonoBehaviour
    {
        [SerializeField] private UIGenerateMap uiGenerateMap;
        
        public void Initialize(Action<int, int> generateMapAction)
        {
            uiGenerateMap.OnGenerateMap += generateMapAction;
        }
    }
}
