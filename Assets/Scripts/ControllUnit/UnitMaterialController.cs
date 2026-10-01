using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public class UnitMaterialController
    {
        private readonly Renderer objectRenderer;
        private readonly MaterialPropertyBlock propertyBlock;
        
        private readonly Material transparent;
        private readonly Material opaque;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private const float TRNASPARENT_VALUE = 0.5f;
        private const float OPAQUE_VALUE = 1f;

        public UnitMaterialController(Unit unit)
        {
            propertyBlock = new MaterialPropertyBlock();
            objectRenderer = unit.GetComponent<Renderer>();
            
            transparent = unit.UnitData.Transparent;
            opaque = unit.UnitData.Opaque;
        }

        public void SetTranslucent()
        {
            objectRenderer.sharedMaterial = transparent;
        }

        public void SetOpaque()
        {
            objectRenderer.sharedMaterial = opaque;
        }
    }
}
