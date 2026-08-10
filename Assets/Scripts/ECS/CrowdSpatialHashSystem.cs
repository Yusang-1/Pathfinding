using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.ECS
{
    public partial struct CrowdSpatialHashSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var cellMap = new NativeParallelMultiHashMap<int, Entity>(1024, Allocator.Temp);

            // SystemAPI.Query : foreach와 함께 사용해 특정 컴포넌트를 가진 엔티티 데이터를 순회
            // WithAll : 지정한 컴포넌트나 태그를 모두 포함하는 엔티티만 걸러냄
            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<MoveUnitTag>().WithEntityAccess())
            {
                int key = GetHashKey(transform.ValueRO.Position);
                cellMap.Add(key, entity);
            }

            var entityManager = state.EntityManager;

            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                .WithAll<MoveUnitTag>()
                .WithEntityAccess())
            {
                DynamicBuffer<NearbyEntityElement> buffer = entityManager.GetBuffer<NearbyEntityElement>(entity);
                buffer.Clear();

                var nearby = new NativeList<Entity>(Allocator.Temp);
                CollectNeighbors(cellMap, transform.ValueRO.Position, nearby);

                for (int i = 0; i < nearby.Length; i++)
                {
                    buffer.Add(new NearbyEntityElement { Value = nearby[i] });
                }

                nearby.Dispose();
            }

            cellMap.Dispose();
        }

        private int GetHashKey(float3 position)
        {
            return (int)math.hash(GetCellCoord(position));
        }

        private int2 GetCellCoord(float3 position)
        {
            int cellSize = 2;
            return new int2(
                (int)math.floor(position.x / cellSize),
                (int)math.floor(position.z / cellSize)
            );
        }

        private void CollectNeighbors(NativeParallelMultiHashMap<int, Entity> cellMap, float3 position, NativeList<Entity> nearby)
        {
            int2 centerCell = GetCellCoord(position);
            int range = 1;

            for (int x = -range; x <= range; x++)
            {
                for (int z = -range; z <= range; z++)
                {
                    int2 neighborCell = centerCell + new int2(x, z);
                    int neighborKey = (int)math.hash(neighborCell);

                    var iterator = cellMap.GetValuesForKey(neighborKey);
                    while (iterator.MoveNext())
                    {
                        nearby.Add(iterator.Current);
                    }
                }
            }
        }
    }
}
