using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit.SO
{
    [CreateAssetMenu(fileName = "UnitBottomSelectSO", menuName = "Scriptable Objects/UnitBottomSelectSO")]
    public class UnitBottomSelectSO : ScriptableObject
    {
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite focusedSprite;

        private Dictionary<UnitBottomStatus, Sprite> spriteGetter = new();

        public void Initialize()
        {
            if (!spriteGetter.ContainsKey(UnitBottomStatus.Selected))
            {
                spriteGetter.Add(UnitBottomStatus.Selected, selectedSprite);
            }
            if (!spriteGetter.ContainsKey(UnitBottomStatus.Focused))
            {
                spriteGetter.Add(UnitBottomStatus.Focused, focusedSprite);
            }
        }

        public Sprite GetSprite(UnitBottomStatus status)
        {
            return spriteGetter[status];
        }
    }
}
