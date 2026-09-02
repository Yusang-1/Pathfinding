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

        private const int CAPACITY = 2048;

        public void OnCreate(ref SystemState state)
        {
            entityManager = state.EntityManager;
            cells = new NativeParallelMultiHashMap<int, Entity>(CAPACITY, Allocator.Persistent);
            registeredEntities = new NativeParallelHashMap<Entity, int>(CAPACITY, Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state)
        {
            if (cells.IsCreated)
            {
                cells.Dispose();
            }

            if (registeredEntities.IsCreated)
            {
                registeredEntities.Dispose();
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

            ManageSelect(ref state);
        }

        private void ManageSelect(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            OnePointSelect(ref state, ecb);

            AreaSelect(ref state, ecb);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void OnePointSelect(ref SystemState state, EntityCommandBuffer ecb)
        {
            if (SystemAPI.TryGetSingleton<UnitSelectionRequest>(out var request))
            {
                if (SystemAPI.TryGetSingletonEntity<UnitSelectionRequest>(out Entity requestEntity))
                {
                    int hash = SpatialHashUtility.GetHash(request.WorldPosition);
                    var values = cells.GetValuesForKey(hash);

                    Entity selectEntity = Entity.Null;
                    float closestDistanceSq = float.MaxValue;
                    foreach (Entity entity in values)
                    {
                        if (!state.EntityManager.Exists(entity)
                            || state.EntityManager.HasComponent<Disabled>(entity)
                            || !state.EntityManager.HasComponent<SelectableUnitTag>(entity))
                        {
                            continue;
                        }
                        var entityTransfrom = state.EntityManager.GetComponentData<LocalTransform>(entity);
                        float distanceSq = math.distancesq(request.WorldPosition, entityTransfrom.Position);

                        if (distanceSq <= closestDistanceSq)
                        {
                            closestDistanceSq = distanceSq;
                            selectEntity = entity;
                        }
                    }

                    if (selectEntity != Entity.Null)
                    {
                        if (request.IsAdditive)
                        {
                            // IsAdditive && 이미 선택중 ecb.RemoveComponent(selectEntity, typeof(SelectedUnitTag));
                            if (state.EntityManager.HasComponent<SelectedUnitTag>(selectEntity))
                            {
                                UnselectEntity(ref state, selectEntity, ecb);
                            }
                            else // IsAdditive && 선택중X ecb.AddComponent(selectEntity, typeof(SelectedUnitTag));
                            {
                                SelectEntity(selectEntity, ecb);
                            }
                        }
                        else // !IsAdditive / 기존 SelectedUnitTag가진 엔티티들 해제, selectedEntity select
                        {
                            UnselectAllEntities(ref state, ecb);

                            SelectEntity(selectEntity, ecb);
                        }
                    }
                    else
                    {
                        UnselectAllEntities(ref state, ecb);
                    }

                    ecb.DestroyEntity(requestEntity);
                }
            }
        }

        private void AreaSelect(ref SystemState state, EntityCommandBuffer ecb)
        {
            if (SystemAPI.TryGetSingleton<UnitAreaSelectionRequest>(out var request))
            {
                if (SystemAPI.TryGetSingletonEntity<UnitAreaSelectionRequest>(out Entity requestEntity))
                {
                    if (request.IsAdditive)
                    {
                        UnselectAllEntities(ref state, ecb);
                    }

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
                                if (!state.EntityManager.HasComponent<LocalTransform>(entity)) continue;

                                float3 position = state.EntityManager.GetComponentData<LocalTransform>(entity).Position;

                                if (position.x >= xMin && position.x <= xMax && position.y >= yMin && position.y <= yMax)
                                {
                                    SelectEntity(entity, ecb);
                                }
                            }
                        }
                    }

                    ecb.DestroyEntity(requestEntity);
                }
            }
        }

        private void SelectEntity(Entity entity, EntityCommandBuffer ecb)
        {
            ecb.AddComponent(entity, typeof(SelectedUnitTag));

            // select후 actionMap 변경
            CreateActionMapRequest(ecb, ActionMaps.Unit);

            // select후 ui 처리
            var select = ecb.CreateEntity();
            var name = entityManager.GetComponentData<ECSUnitComponent>(entity).UnitName;
            ecb.AddComponent(select, new UnitSelectedData() { Entity = entity, EntityName = name });
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

    public struct UnitSelectionRequest : IComponentData
    {
        public float3 WorldPosition;

        /// <summary> Shift Click 여부 </summary>
        public bool IsAdditive;
    }

    public struct UnitAreaSelectionRequest : IComponentData
    {
        public float3 StandardPosition;
        public float Width;
        public float Height;
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

    public struct SelectableUnitTag : IComponentData { }

    public struct ChangeActionMapRequest : IComponentData
    {
        public ActionMaps TargetMap;
    }
}
