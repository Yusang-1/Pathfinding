using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSInputManager : MonoBehaviour
    {
        public event Action<Vector3> OnHoldStarted;
        public event Action<Vector3> OnHoldPerformed;
        public event Action OnHoldCanceled;
        public event Action OnControllMenu;
        public event Action<Vector3> OnSetSpawnAreaRequested;

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private PlayerInput playerInputComponent;
        [SerializeField] private ECSPlayerControllInput playerControllInput;
        [SerializeField] private ECSUnitInput unitInput;
        [SerializeField] private SpawnAreaSetterInput spawnAreaSetterInput;

        private InputActionMap actionMap;
        private IActionMapInputer currentInputer;
        private readonly Dictionary<ActionMaps, string> actionMapNameDict = new();
        private readonly Dictionary<ActionMaps, IActionMapInputer> inputerDict = new();
        
        private EntityQuery changeActionMapQuery;

        private bool isEventBound;

        private void Awake()
        {
            actionMap = inputActions.actionMaps[0];
            actionMap.Enable();
        }

        private void OnEnable()
        {
            BindEvents();
        }

        private void Start()
        {
            changeActionMapQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(ChangeActionMapRequest));
            
            actionMapNameDict.Add(ActionMaps.Player, "Player");
            actionMapNameDict.Add(ActionMaps.Unit, "Unit");
            actionMapNameDict.Add(ActionMaps.SpawnAreaSetter, "SpawnAreaSetter");

            inputerDict.Add((playerControllInput as IActionMapInputer).GetActionMap(), playerControllInput);
            inputerDict.Add((unitInput as IActionMapInputer).GetActionMap(), unitInput);
            inputerDict.Add((spawnAreaSetterInput as IActionMapInputer).GetActionMap(), spawnAreaSetterInput);

            ChangeActionMapDefault();
        }
        
        private void Update()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            using var requests = changeActionMapQuery.ToComponentDataArray<ChangeActionMapRequest>(Allocator.Temp);
            
            using var entities = changeActionMapQuery.ToEntityArray(Allocator.Temp);
            
            for(int i = 0; i < requests.Length; i++)
            {
                ChangeActionMapSelected(requests[i].TargetMap);
                entityManager.DestroyEntity(entities[i]);
            }
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        public void Initialize(ECSSelectableController selectableController)
        {
            selectableController.GetActions(ChangeActionMapSelected, ChangeActionMapDefault);

            playerControllInput.Initialize(selectableController);
            unitInput.Initialize(selectableController);
        }

        public void ChangeActionMapSelected(ActionMaps actionMap)
        {
            string actionMapName = actionMapNameDict[actionMap];

            playerInputComponent.SwitchCurrentActionMap(actionMapName);

            currentInputer?.ActionMapDeactivated();

            currentInputer = inputerDict[actionMap];

            currentInputer.ActionMapActivated();
        }

        private const ActionMaps DefaultActionMap = ActionMaps.Player;
        private void ChangeActionMapDefault()
        {
            ChangeActionMapSelected(DefaultActionMap);
        }

        private void BindEvents()
        {
            if (isEventBound) return;

            playerControllInput.OnHoldStarted += HandlerHoldStarted;
            playerControllInput.OnHoldPerformed += HandlerHoldPerformed;
            playerControllInput.OnHoldCanceled += HandlerHoldCanceled;
            playerControllInput.OnControllMenu += HandlerControllMenu;

            unitInput.OnHoldStarted += HandlerHoldStarted;
            unitInput.OnHoldPerformed += HandlerHoldPerformed;
            unitInput.OnHoldCanceled += HandlerHoldCanceled;
            unitInput.OnControllMenu += HandlerControllMenu;

            spawnAreaSetterInput.OnSetSpawnAreaRequested += HandlerSetSpawnAreaRequested;
            spawnAreaSetterInput.OnSetSpawnAreaFinished += ChangeActionMapDefault;

            isEventBound = true;
        }

        private void UnbindEvents()
        {
            if (!isEventBound) return;

            playerControllInput.OnHoldStarted -= HandlerHoldStarted;
            playerControllInput.OnHoldPerformed -= HandlerHoldPerformed;
            playerControllInput.OnHoldCanceled -= HandlerHoldCanceled;
            playerControllInput.OnControllMenu -= HandlerControllMenu;

            unitInput.OnHoldStarted -= HandlerHoldStarted;
            unitInput.OnHoldPerformed -= HandlerHoldPerformed;
            unitInput.OnHoldCanceled -= HandlerHoldCanceled;
            unitInput.OnControllMenu -= HandlerControllMenu;

            spawnAreaSetterInput.OnSetSpawnAreaRequested -= HandlerSetSpawnAreaRequested;
            spawnAreaSetterInput.OnSetSpawnAreaFinished -= ChangeActionMapDefault;

            isEventBound = false;
        }

        private void HandlerHoldStarted(Vector3 vec)
        {
            OnHoldStarted?.Invoke(vec);
        }
        private void HandlerHoldPerformed(Vector3 vec)
        {
            OnHoldPerformed?.Invoke(vec);
        }
        private void HandlerHoldCanceled()
        {
            OnHoldCanceled?.Invoke();
        }
        private void HandlerControllMenu()
        {
            OnControllMenu?.Invoke();
        }
        private void HandlerSetSpawnAreaRequested(Vector3 vec)
        {
            OnSetSpawnAreaRequested?.Invoke(vec);
        }
    }
}

public enum ActionMaps
{
    Player,
    Unit,
    SpawnAreaSetter
}