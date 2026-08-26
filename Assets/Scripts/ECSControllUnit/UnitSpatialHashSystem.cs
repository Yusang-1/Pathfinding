using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.ECSControllUnit
{
    public partial struct UnitSpatialHashSystem : ISystem
    {
        private NativeParallelMultiHashMap<int, Entity> cells;
        private NativeParallelHashMap<Entity, int> registeredEntities;

        private const int CAPACITY = 2048;

        public void OnCreate(ref SystemState state)
        {
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

                if (!state.EntityManager.Exists(entityRegistered))
                {
                    removedEntities.Add((entityRegistered, cellRegistered));                    
                }
            }
            
            foreach(var (entity, cell) in removedEntities)
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
                    Register(entity, out int newCell, position);
                    cell.ValueRW.IsRegistered = 1;
                    cell.ValueRW.Value = newCell;
                }
                else
                {
                    Update(entity, prevCell, out int newCell, position);
                    cell.ValueRW.Value = newCell;
                }
            }
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

        public void Clear()
        {
            cells.Clear();
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
}
