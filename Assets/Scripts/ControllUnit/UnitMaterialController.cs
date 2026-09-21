using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public class UnitMaterialController
    {
        private readonly Unit unit;

        private Renderer objectRenderer;
        private readonly MaterialPropertyBlock propertyBlock;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private const float TRNASPARENT_VALUE = 0.5f;
        private const float OPAQUE_VALUE = 1f;

        public UnitMaterialController(Unit unit)
        {
            this.unit = unit;
            propertyBlock = new MaterialPropertyBlock();
            objectRenderer = unit.GetComponent<Renderer>();
        }

        public void SetTranslucent()
        {
            objectRenderer.GetPropertyBlock(propertyBlock);

            Color color = objectRenderer.sharedMaterial.GetColor(BaseColor);
            color.a = Mathf.Clamp01(TRNASPARENT_VALUE);

            propertyBlock.SetColor(BaseColor, color);
            objectRenderer.SetPropertyBlock(propertyBlock);
        }

        public void SetOpaque()
        {
            objectRenderer.GetPropertyBlock(propertyBlock);

            Color color = objectRenderer.sharedMaterial.GetColor(BaseColor);
            color.a = Mathf.Clamp01(OPAQUE_VALUE);

            propertyBlock.SetColor(BaseColor, color);
            objectRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
