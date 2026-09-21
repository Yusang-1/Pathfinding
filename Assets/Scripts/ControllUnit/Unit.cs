using UnityEngine;
using System;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class Unit : MonoBehaviour, ISelectableUnit, IHaveOwnActionMap, IPoolObject<Unit>
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;
        public event Action<ISelectableUnit> OnDespawnedCallback;
        public event Action<Unit> OnPoolObjectUnused;

        [SerializeField] private UnitSO unitData;
        [SerializeField] private SteeringWeightingSO steeringWeightingData;

        private UnitController controller;
        private UnitBottomSelectChanger bottomChanger;

        private UnitBottomStatus bottomStatus;
        public Vector2Int CurrentKey;

        public UnitSO UnitData => unitData;
        public UnitController Controller => controller;

        public bool IsEventBound;
        private bool isSpawned;

        private void Update()
        {
            if (!isSpawned) return;

            controller.ControllerUpdate();
        }

        private void LateUpdate()
        {
            if (!isSpawned) return;

            controller.ControllerLateUpdate();
        }

        public void Initialize(UnitRuntimeContext unitRuntimeContext, UnitBottomSelectChanger bottomChanger)
        {
            controller = new UnitController(this, unitRuntimeContext, bottomChanger.transform, steeringWeightingData.WalkConfig, unitData); this.bottomChanger = bottomChanger;
            bottomChanger.SetRadius(unitData.Radius);
        }

        public void UnitSpawned()
        {
            isSpawned = true;

            bottomChanger.Initialize();
            gameObject.SetActive(true);
        }

        public void UnitDespawned()
        {
            OnDespawnedCallback?.Invoke(this);

            if (bottomChanger != null)
            {
                bottomChanger.Despawned();
                ChangeBottomStatus(UnitBottomStatus.None);
            }

            gameObject.SetActive(false);
            OnPoolObjectUnused?.Invoke(this);
            isSpawned = false;
        }

        public void Selected()
        {
            ChangeBottomStatus(UnitBottomStatus.Selected);
            OnSelectedCallback?.Invoke(this);
        }

        public void Deselected()
        {
            ChangeBottomStatus(UnitBottomStatus.None);
            OnDeselectedCallback?.Invoke(this);
        }

        public void Focused()
        {
            if (bottomStatus == UnitBottomStatus.Selected) return;

            ChangeBottomStatus(UnitBottomStatus.Focused);
        }

        public void Unfocused()
        {
            if (bottomStatus == UnitBottomStatus.Selected) return;

            ChangeBottomStatus(UnitBottomStatus.None);
        }

        public SelectableType GetSelectableType() => unitData.SelectableType;

        private void ChangeBottomStatus(UnitBottomStatus status)
        {
            bottomStatus = status;
            bottomChanger.StatusChanged(bottomStatus);
        }

        public ActionMaps GetActionMapName() => unitData.ActionMap;
    }

    // public class UnitMaterialController
    // {
    //     private readonly Unit unit;
        
    //     private Renderer objectRenderer;
    //     private readonly MaterialPropertyBlock propertyBlock;

    //     private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    //     public UnitMaterialController(Unit unit)
    //     {
    //         this.unit = unit;
    //         propertyBlock = new MaterialPropertyBlock();
    //     }
        
    //     private void SetTranslucent()
    //     {
    //         objectRenderer = unit.GetComponent<Renderer>();

    //         objectRenderer.GetPropertyBlock(propertyBlock);

    //         Color color = propertyBlock.GetColor(BaseColor);
    //         color.a = Mathf.Clamp01(0.5f);

    //         propertyBlock.SetColor(BaseColor, color);
    //         objectRenderer.SetPropertyBlock(propertyBlock);
    //     }

    //     private void SetOpaque()
    //     {
    //         objectRenderer.GetPropertyBlock(propertyBlock);

    //         Color color = propertyBlock.GetColor(BaseColor);
    //         color.a = Mathf.Clamp01(1f);

    //         propertyBlock.SetColor(BaseColor, color);
    //         objectRenderer.SetPropertyBlock(propertyBlock);
    //     }
    // }
}

public interface IHaveOwnActionMap
{
    public ActionMaps GetActionMapName();
}
