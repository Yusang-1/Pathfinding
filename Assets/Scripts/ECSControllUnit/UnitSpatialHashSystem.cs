using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.ECSControllUnit
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(UnitMoveSystem))]
    public partial struct UnitSpatialHashSystem : ISystem
    {
        private EntityManager entityManager;

        private NativeParallelMultiHashMap<int, Entity> cells;
        private NativeParallelHashMap<Entity, int> registeredEntities;
        // private NativeList<Entity> focusedEntities;
        private NativeHashSet<Entity> focusedEntities;
        private NativeHashSet<Entity> focusedEntitiesInThisFrame;

        private const int CAPACITY = 2048;

        public void OnCreate(ref SystemState state)
        {
            entityManager = state.EntityManager;
            cells = new NativeParallelMultiHashMap<int, Entity>(CAPACITY, Allocator.Persistent);
            registeredEntities = new NativeParallelHashMap<Entity, int>(CAPACITY, Allocator.Persistent);
            focusedEntities = new NativeHashSet<Entity>(CAPACITY, Allocator.Persistent);
            focusedEntitiesInThisFrame = new NativeHashSet<Entity>(CAPACITY, Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state)
        {
            if (cells.IsCreated)
            {
                cells.Dispose();
                cells = default;
            }

            if (registeredEntities.IsCreated)
            {
                registeredEntities.Dispose();
                registeredEntities = default;
            }

            if (focusedEntities.IsCreated)
            {
                focusedEntities.Dispose();
                focusedEntities = default;
            }

            if (focusedEntitiesInThisFrame.IsCreated)
            {
                focusedEntitiesInThisFrame.Dispose();
                focusedEntitiesInThisFrame = default;
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            // 제거된 entity SpatialHash에서 제거
            var removedEntities = new NativeList<(Entity entity, int cell)>(Allocator.Temp);

            foreach (var pair in registeredEntities)
            {
                Entity entityRegistered = pair.Key;
                int cellRegistered = pair.Value;

                if (!state.EntityManager.Exists(entityRegistered) || state.EntityManager.HasComponent<Disabled>(entityRegistered))
                {
                    removedEntities.Add((entityRegistered, cellRegistered));
                }
            }

            foreach (var (entity, cell) in removedEntities)
            {
                Remove(entity, cell);
            }

            removedEntities.Dispose();

            // 움직인 entity의 cell 갱신
            foreach (var (transform, cell, entity) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRW<SpatialHashCell>>()
                    .WithAll<ECSUnitComponent>()
                    .WithEntityAccess())
            {
                int prevCell = cell.ValueRO.Value;
                byte isRegistered = cell.ValueRW.IsRegistered;

                float3 position = transform.ValueRO.Position;

                if (isRegistered == 0)
                {
                    Register(entity, out int newCellHash, position);
                    cell.ValueRW.IsRegistered = 1;
                    cell.ValueRW.Value = newCellHash;
                }
                else
                {
                    Update(entity, prevCell, out int newCellHash, position);
                    cell.ValueRW.Value = newCellHash;
                }
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            CheckFocused(ecb);

            SelectFocused(ref state, ecb);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void CheckFocused(EntityCommandBuffer ecb)
        {
            focusedEntitiesInThisFrame.Clear();

            // area focus check를 먼저 진행
            bool hasCheckAreaFocusRequest = SystemAPI.TryGetSingleton<CheckUnitAreaFocusedRequest>(out var areaRequest);
            if (hasCheckAreaFocusRequest)
            {
                if (SystemAPI.TryGetSingletonEntity<CheckUnitAreaFocusedRequest>(out Entity areaRequestEntity))
                {
                    CheckAreaFocused(ecb, areaRequest);
                    ecb.DestroyEntity(areaRequestEntity);
                }
            }

            if (SystemAPI.TryGetSingleton<CheckUnitFocusedRequest>(out var request))
            {
                if (SystemAPI.TryGetSingletonEntity<CheckUnitFocusedRequest>(out Entity requestEntity))
                {
                    // area focus를 진행중에는 point focus check를 하지 않음
                    if (!hasCheckAreaFocusRequest)
                    {
                        CheckPointFocused(ecb, request);
                    }
                    ecb.DestroyEntity(requestEntity);
                }
            }
        }

        private void CheckPointFocused(EntityCommandBuffer ecb, CheckUnitFocusedRequest request)
        {
            int hash = SpatialHashUtility.GetHash(request.WorldPosition);
            var entitiesInCell = cells.GetValuesForKey(hash);

            Entity focusedEntity = Entity.Null;
            float closestDistanceSq = float.MaxValue;
            foreach (Entity entity in entitiesInCell)
            {
                if (!entityManager.Exists(entity)
                    || entityManager.HasComponent<Disabled>(entity)
                    || !entityManager.HasComponent<SelectableUnitTag>(entity))
                {
                    continue;
                }

                var entityTransfrom = entityManager.GetComponentData<LocalTransform>(entity);
                float distanceSq = math.distancesq(request.WorldPosition, entityTransfrom.Position);

                if (distanceSq <= closestDistanceSq)
                {
                    closestDistanceSq = distanceSq;
                    focusedEntity = entity;
                }
            }

            if (focusedEntity != Entity.Null)
            {
                // FocusedUnitTag, SelectedUnitTag가 없으면 focus 처리
                if (!entityManager.HasComponent<FocusedUnitTag>(focusedEntity) && !entityManager.HasComponent<SelectedUnitTag>(focusedEntity))
                {
                    LocalTransform transform = entityManager.GetComponentData<LocalTransform>(focusedEntity);
                    FocusEntityInThisFrame(focusedEntity);
                    CompareFocused(ecb);
                }
            }
            else // 새로 focused된 엔티티가 없는 경우
            {
                using var focusedSnapshot = new NativeList<Entity>(focusedEntities.Count, Allocator.Temp);

                foreach (Entity entity in focusedEntities)
                {
                    focusedSnapshot.Add(entity);
                }

                foreach (Entity entity in focusedSnapshot)
                {
                    if (!entityManager.Exists(entity))
                    {
                        focusedEntities.Remove(entity);
                        continue;
                    }

                    // entity가 focused가 아니고 Selected이면 continue
                    if (!entityManager.HasComponent<FocusedUnitTag>(entity) && entityManager.HasComponent<SelectedUnitTag>(entity))
                    {
                        continue;
                    }

                    UnfocusEntity(entity, ecb);
                }
            }
        }

        private void CheckAreaFocused(EntityCommandBuffer ecb, CheckUnitAreaFocusedRequest request)
        {
            float3 standard = request.StandardPosition;
            float width = request.Width;
            float height = request.Height;

            float xMin = math.min(standard.x, standard.x + width);
            float yMin = math.min(standard.y, standard.y + height);
            float xMax = xMin + math.abs(width);
            float yMax = yMin + math.abs(height);

            int2 minCell = SpatialHashUtility.GetCell(new float3 { x = xMin, y = yMin, z = standard.z });
            int2 maxCell = SpatialHashUtility.GetCell(new float3 { x = xMax, y = yMax, z = standard.z });

            int hash;
            NativeParallelMultiHashMap<int, Entity>.Enumerator entities;

            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                if (x < 0) continue;
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    if (y < 0) continue;
                    hash = SpatialHashUtility.GetHash(new int2 { x = x, y = y });
                    entities = cells.GetValuesForKey(hash);

                    foreach (Entity entity in entities)
                    {
                        if (!entityManager.Exists(entity)
                            || entityManager.HasComponent<Disabled>(entity)
                            || !entityManager.HasComponent<SelectableUnitTag>(entity))
                        {
                            continue;
                        }

                        LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(entity);
                        float3 position = localTransform.Position;

                        if (position.x >= xMin && position.x <= xMax && position.y >= yMin && position.y <= yMax)
                        {
                            FocusEntityInThisFrame(entity);
                        }
                    }
                }
            }

            CompareFocused(ecb);
        }

        private void SelectFocused(ref SystemState state, EntityCommandBuffer ecb)
        {
            if (!SystemAPI.TryGetSingleton<UnitSelectionRequest>(out var request) ||
                !SystemAPI.TryGetSingletonEntity<UnitSelectionRequest>(out Entity requestEntity))
            {
                return;
            }

            if (!request.IsAdditive)
            {
                UnselectAllEntities(ref state, ecb);
            }

            using var focusedSnapshot = new NativeList<Entity>(focusedEntities.Count, Allocator.Temp);

            foreach (Entity entity in focusedEntities)
            {
                focusedSnapshot.Add(entity);
            }

            foreach (Entity entity in focusedSnapshot)
            {
                if (entity == Entity.Null || !entityManager.Exists(entity))
                {
                    continue;
                }

                LocalTransform transform = entityManager.GetComponentData<LocalTransform>(entity);

                if (request.IsAdditive)
                {
                    // IsAdditive && 이미 선택중 ecb.RemoveComponent(selectEntity, typeof(SelectedUnitTag));
                    if (entityManager.HasComponent<SelectedUnitTag>(entity))
                    {
                        UnselectEntity(ref state, entity, ecb);
                    }
                    else // IsAdditive && 선택중X ecb.AddComponent(selectEntity, typeof(SelectedUnitTag));
                    {
                        SelectEntity(entity, ecb, transform);
                    }
                }
                else // !IsAdditive / selectedEntity select
                {
                    SelectEntity(entity, ecb, transform);
                }

            }

            ecb.DestroyEntity(requestEntity);
        }

        private void FocusEntityInThisFrame(Entity entity)
        {
            if (focusedEntitiesInThisFrame.Contains(entity)) return;

            focusedEntitiesInThisFrame.Add(entity);
        }

        private void FocusEntity(Entity entity, EntityCommandBuffer ecb, LocalTransform localTransform)
        {
            if (focusedEntities.Contains(entity)) return;

            ecb.AddComponent<FocusedUnitTag>(entity);
            focusedEntities.Add(entity);

            var component = entityManager.GetComponentData<ECSUnitComponent>(entity);
            ActiveUnitBottom(ecb, component, localTransform);
        }

        private void UnfocusEntity(Entity entity, EntityCommandBuffer ecb)
        {
            if (!focusedEntities.Contains(entity)) return;

            focusedEntities.Remove(entity);

            if (!entityManager.Exists(entity)) return;

            if (entityManager.HasComponent<FocusedUnitTag>(entity))
            {
                ecb.RemoveComponent<FocusedUnitTag>(entity);
            }

            // 유닛 하단 표시 엔티티 해제
            var component = entityManager.GetComponentData<ECSUnitComponent>(entity);
            DeactiveUnitBottom(component, ecb);
        }

        /// <summary> focused와 focused in this frame을 비교해 focus, unfocus한다. </summary>        
        private void CompareFocused(EntityCommandBuffer ecb)
        {
            using NativeList<Entity> unfocusEntities = new(Allocator.Temp);
            // 새로 focus된 엔티티에 포함되지 않은 이전 focus 엔티티를 native list에 저장
            foreach (Entity focusedBefore in focusedEntities)
            {
                if (!focusedEntitiesInThisFrame.Contains(focusedBefore))
                {
                    unfocusEntities.Add(focusedBefore);
                }
            }

            // 새로 focus된 엔티티를 focus처리
            foreach (Entity focusedNew in focusedEntitiesInThisFrame)
            {
                if (focusedEntities.Contains(focusedNew)) continue;

                FocusEntity(focusedNew, ecb, entityManager.GetComponentData<LocalTransform>(focusedNew));
            }

            // native list에 저장해둔 unfocus할 엔티티를 unfocus
            foreach (Entity entity in unfocusEntities)
            {
                UnfocusEntity(entity, ecb);
            }
        }

        private void SelectEntity(Entity entity, EntityCommandBuffer ecb, LocalTransform localTransform) // ActiveUnitBottom을 스프라이트 변경으로 바꿔야함
        {
            UnfocusEntity(entity, ecb);
            ecb.AddComponent(entity, typeof(SelectedUnitTag));

            // select후 actionMap 변경
            CreateActionMapRequest(ecb, ActionMaps.Unit);

            // select후 ui 처리
            var select = ecb.CreateEntity();
            var component = entityManager.GetComponentData<ECSUnitComponent>(entity);
            var name = component.UnitName;
            ecb.AddComponent(select, new UnitSelectedData() { Entity = entity, EntityName = name });

            ActiveUnitBottom(ecb, component, localTransform);
        }

        private void UnselectEntity(ref SystemState state, Entity entity, EntityCommandBuffer ecb)
        {
            ecb.RemoveComponent(entity, typeof(SelectedUnitTag));

            // SelectedUnitTag를 가진 엔티티가 없다면 actionMap변경
            bool hasOtherSelectedEntity = false;

            foreach (var (tag, selectedEntity) in SystemAPI.Query<RefRO<SelectedUnitTag>>().WithEntityAccess())
            {
                if (selectedEntity != entity)
                {
                    hasOtherSelectedEntity = true;
                    break;
                }
            }
            if (!hasOtherSelectedEntity)
            {
                CreateActionMapRequest(ecb, ActionMaps.Player);
            }

            // unselect후 ui 처리
            var select = ecb.CreateEntity();
            ecb.AddComponent(select, new UnitDeselectedData() { Entity = entity });

            var component = entityManager.GetComponentData<ECSUnitComponent>(entity);
            DeactiveUnitBottom(component, ecb);
        }

        private void UnselectAllEntities(ref SystemState state, EntityCommandBuffer ecb)
        {
            var registered = registeredEntities.GetKeyArray(Allocator.Temp);

            foreach (var (tag, entity) in SystemAPI.Query<RefRO<SelectedUnitTag>>().WithEntityAccess())
            {
                UnselectEntity(ref state, entity, ecb);
            }

            registered.Dispose();
        }

        /// <summary> 유닛 하단 표시를 활성화하고 위치 지정 </summary>
        private void ActiveUnitBottom(EntityCommandBuffer ecb, ECSUnitComponent component, LocalTransform entityTransform) // 스프라이트를 지정할수 있도록 해야함
        {
            var bottomEntity = component.BottomCircle;

            ecb.RemoveComponent(bottomEntity, typeof(Disabled));
            ecb.SetComponent<LocalTransform>(bottomEntity, LocalTransform.FromPosition(entityTransform.Position));
        }

        private void DeactiveUnitBottom(ECSUnitComponent component, EntityCommandBuffer ecb)
        {
            var bottomEntity = component.BottomCircle;

            ecb.AddComponent(bottomEntity, typeof(Disabled));
        }

        private void CreateActionMapRequest(EntityCommandBuffer ecb, ActionMaps actionMap)
        {
            Entity requestEntity = ecb.CreateEntity();

            ecb.AddComponent(
                requestEntity,
                new ChangeActionMapRequest
                {
                    TargetMap = actionMap
                }
            );
        }

        public void Register(Entity entity, out int newCell, float3 position)
        {
            newCell = SpatialHashUtility.GetHash(position);

            cells.Add(newCell, entity);
            registeredEntities.Add(entity, newCell);
        }

        public void Update(Entity entity, int prevCell, out int newCell, float3 position)
        {
            newCell = SpatialHashUtility.GetHash(position);

            // 같은 cell이면 return
            if (prevCell == newCell)
            {
                return;
            }

            cells.Remove(prevCell, entity);
            cells.Add(newCell, entity);
            registeredEntities[entity] = newCell;
        }

        public void Remove(Entity entity, int cell)
        {
            cells.Remove(cell, entity);
            registeredEntities.Remove(entity);
        }

        public NativeParallelMultiHashMap<int, Entity>.Enumerator GetCellEntities(int2 cell)
        {
            return cells.GetValuesForKey(SpatialHashUtility.GetHash(cell));
        }
    }

    public struct SpatialHashCell : IComponentData
    {
        public int Value;
        public byte IsRegistered;
    }

    public struct SelectedUnitTag : IComponentData { }

    public struct FocusedUnitTag : IComponentData { }

    public struct UnitSelectionRequest : IComponentData
    {
        public float3 WorldPosition;

        /// <summary> Shift Click 여부 </summary>
        public bool IsAdditive;
    }

    public struct UnitSelectedData : IComponentData
    {
        public FixedString32Bytes EntityName;
        public Entity Entity;
    }

    public struct UnitDeselectedData : IComponentData
    {
        public Entity Entity;
    }

    public struct CheckUnitFocusedRequest : IComponentData
    {
        public float3 WorldPosition;
    }

    public struct CheckUnitAreaFocusedRequest : IComponentData
    {
        public float3 StandardPosition;
        public float Width;
        public float Height;
        public bool IsAdditive;
    }

    public struct SelectableUnitTag : IComponentData { }

    public struct ChangeActionMapRequest : IComponentData
    {
        public ActionMaps TargetMap;
    }
}
