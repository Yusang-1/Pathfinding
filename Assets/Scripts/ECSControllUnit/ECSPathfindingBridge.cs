using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System.Collections.Generic;
using Assets.Scripts.Pathfinding;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSPathfindingBridge : MonoBehaviour
    {
        private NodeList nodeList;
        private HPAClusterList clusterList;

        private AStarPathfinder aStarPathfinder;
        private HPAPathfinder highLevelPathfinder;        
        private ClusterPathSmoother clusterPathSmoother;
        private SearchWithTheClusterResult searchWithTheClusterResult;
        private readonly ClusterResultWrapper clusterResultWrapper = new();

        private EntityManager entityManager;

        private List<ClusterSmootherResult> abstractPaths = new();
        private readonly List<Vector2Int> clusterIndexes = new();


        private void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;            
        }

        private void Update()
        {
            HandleLazyRefine();
        }
        
        private void HandleLazyRefine()
        {
            EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(UnitMoveState));

            using NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.TempJob);

            DynamicBuffer<HighLevelWaypoint> highLevelWaypointBuffer;
            DynamicBuffer<HighLevelClusterPath> highLevelClusterPathBuffer;
            DynamicBuffer<LowLevelWaypoint> lowLevelWaypointBuffer;

            foreach (Entity entity in entities)
            {
                var moveState = entityManager.GetComponentData<UnitMoveState>(entity);

                if (moveState.IsMoving && moveState.IsNeedLazyRefine)
                {
                    // 다음 high level path 찾음
                    int nextHighLevelPathIndex = moveState.HighLevelPathIndex + 1;

                    highLevelWaypointBuffer = entityManager.GetBuffer<HighLevelWaypoint>(entity);
                    highLevelClusterPathBuffer = entityManager.GetBuffer<HighLevelClusterPath>(entity);
                    
                    // 버퍼에 다음 path가 없으면 continue
                    if(nextHighLevelPathIndex >= highLevelWaypointBuffer.Length)
                    {
                        continue;
                    }
                    
                    var path = highLevelWaypointBuffer[nextHighLevelPathIndex];

                    clusterIndexes.Clear();
                    int first = path.FirstClusterIndex;
                    int last = first + path.ClusterCount - 1;

                    for (int i = first; i <= last; i++)
                    {
                        int2 index = highLevelClusterPathBuffer[i].ClusterIndex;

                        clusterIndexes.Add(new Vector2Int(index.x, index.y));
                    }

                    // low level경로 탐색
                    float unitRadius = entityManager.GetComponentData<ECSUnitComponent>(entity).Radius;

                    Vector2Int enterNode = new(path.EnterNodeIndex.x, path.EnterNodeIndex.y);
                    Vector2Int exitNode = new(path.ExitNodeIndex.x, path.ExitNodeIndex.y);
                    List<Vector3> nextPath = searchWithTheClusterResult.FindPathThetaWithClusterList(clusterIndexes, enterNode, exitNode, unitRadius);

                    // low level 경로를 버퍼에 추가
                    lowLevelWaypointBuffer = entityManager.GetBuffer<LowLevelWaypoint>(entity);
                    lowLevelWaypointBuffer.RemoveRange(0, lowLevelWaypointBuffer.Length - 1); // 마지막 요소만 남기고 삭제
                    foreach (Vector3 point in nextPath)
                    {
                        lowLevelWaypointBuffer.Add(new LowLevelWaypoint { Position = point });
                    }

                    entityManager.SetComponentData(entity,
                        new UnitMoveState()
                        {
                            IsMoving = true,
                            IsNeedLazyRefine = false,
                            HighLevelPathIndex = nextHighLevelPathIndex,
                            LowLevelPathIndex = 0
                        }
                    );
                }
            }
        }

        public void SetNodeAndCluster(NodeList nodes, in MapData mapData, Dictionary<UnitSize, float> unitRadiusList)
        {
            nodeList = nodes;
            clusterList = new HPAClusterList(nodeList);

            aStarPathfinder = new(nodeList);
            ThetaStar thetaStarPathfinder = new(nodeList);

            clusterList.Initialize(aStarPathfinder, mapData.MapSize, mapData.ClusterSize, unitRadiusList);
            nodeList.SetNodeArea();            
            
            clusterPathSmoother = new ClusterPathSmoother(nodeList, clusterList);
            highLevelPathfinder = new HPAPathfinder(nodeList, clusterList);
            searchWithTheClusterResult = new SearchWithTheClusterResult(aStarPathfinder, thetaStarPathfinder, clusterList, nodeList);
        }

        public void Move(Vector3 to)
        {
            EntityQuery entityQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<SelectedUnitTag>());
            using NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.TempJob);

            LocalTransform transform;
            ECSUnitComponent unitComponent;

            foreach (var entity in entities)
            {
                transform = entityManager.GetComponentData<LocalTransform>(entity);
                unitComponent = entityManager.GetComponentData<ECSUnitComponent>(entity);
                Pathfinding(transform.Position, to, unitComponent.Radius, entity);
            }
        }

        public void MoveAdditive(Vector3 to)
        {

        }

        private void Pathfinding(Vector3 from, Vector3 to, float unitRadius, Entity entity)
        {
            // high level 경로 탐색
            var clusterResultWrapper = GetAbstractPath(from, to, unitRadius);
            abstractPaths = clusterResultWrapper.ClusterSmootherResult;

            if (abstractPaths == null || abstractPaths.Count == 0)
            {
                return;
            }

            // 첫 구간에 대한 low level 경로 탐색
            List<Vector3> resultPath = searchWithTheClusterResult.FindPathThetaWithClusterList(abstractPaths[0], unitRadius);
            if (resultPath == null || resultPath.Count == 0)
            {
                return;
            }

            // 엔티티에 버퍼 값 추가
            DynamicBuffer<HighLevelWaypoint> highLevelWaypointBuffer = entityManager.GetBuffer<HighLevelWaypoint>(entity);
            DynamicBuffer<HighLevelClusterPath> highLevelClusterPathBuffer = entityManager.GetBuffer<HighLevelClusterPath>(entity);
            
            highLevelWaypointBuffer.Clear();
            highLevelClusterPathBuffer.Clear();
            
            // abstractPath는 입구,출구 노드 인덱스, 입구에서 출구까지 경로에 포함되는 Cluster 인덱스를 가짐
            int firstClusterIndex = 0;
            foreach (var abstractPath in abstractPaths)
            {
                int clusterCount = abstractPath.ClusterIndexes.Count;

                // 경로에 포함된 Cluster Index들을 추가
                foreach (var clusterIndex in abstractPath.ClusterIndexes)
                {
                    highLevelClusterPathBuffer.Add(
                        new HighLevelClusterPath()
                        {
                            ClusterIndex = new int2(clusterIndex.x, clusterIndex.y)
                        }
                    );
                }

                Vector2Int enter = abstractPath.EnterNodeIndex;
                Vector2Int exit = abstractPath.ExitNodeIndex;

                highLevelWaypointBuffer.Add(
                    new HighLevelWaypoint()
                    {
                        EnterNodeIndex = new int2(enter.x, enter.y),
                        ExitNodeIndex = new int2(exit.x, exit.y),
                        ClusterCount = clusterCount,
                        FirstClusterIndex = firstClusterIndex
                    }
                );
                firstClusterIndex += clusterCount;
            }

            // 첫 구간에 대한 low level 버퍼 삽입
            DynamicBuffer<LowLevelWaypoint> LowLevelWaypointBuffer = entityManager.GetBuffer<LowLevelWaypoint>(entity);
            LowLevelWaypointBuffer.Clear();
            
            foreach (var position in resultPath)
            {
                LowLevelWaypointBuffer.Add(new LowLevelWaypoint { Position = position });
            }

            entityManager.SetComponentData(entity,
                new UnitMoveState()
                {
                    IsMoving = true,
                    IsNeedLazyRefine = false,
                    HighLevelPathIndex = 0,
                    LowLevelPathIndex = 0
                }
            );

        }

        private ClusterResultWrapper GetAbstractPath(Vector3 from, Vector3 to, float unitRadius)
        {
            clusterResultWrapper.Reset();
            clusterResultWrapper.SetStart(from, to, unitRadius);

            var clusterPath = highLevelPathfinder.FindClusterPath(clusterResultWrapper);
            var smootherClusterPath = clusterPathSmoother.SmoothClusterPath(clusterPath);
            return smootherClusterPath;
        }
    }

    public struct HighLevelWaypoint : IBufferElementData
    {
        public int2 EnterNodeIndex;
        public int2 ExitNodeIndex;

        /// <summary> Index부터 Index까지 이동하는데 몇개의 Cluster를 거치는지 </summary>
        public int ClusterCount;

        /// <summary> 첫 cluster index가 몇인지 </summary>
        public int FirstClusterIndex;
    }

    public struct HighLevelClusterPath : IBufferElementData
    {
        public int2 ClusterIndex;
    }

    public struct LowLevelWaypoint : IBufferElementData
    {
        public float3 Position;
    }

    public struct UnitMoveState : IComponentData
    {
        public bool IsMoving;
        public bool IsNeedLazyRefine;

        public int HighLevelPathIndex;
        public int LowLevelPathIndex;
    }
}

// PathfindingBridge.Pathfinding(Vector3 from, Vector3 to, float unitRadius)
// 1. high level 경로 탐색
// 2. 첫 구간에 대한 low level 경로 탐색
// 3. 엔티티에 버퍼 값 추가

// UnitMoveSystem
// 1. 이동 로직 실행
// 2. 목적지와의 거리가 일정 이하일때 버퍼에서 목적지 받아옴 (현재 몇번째 버퍼 요소를 목적지로 사용하고 있는지 인덱스가 필요할지도)
// 2. 엔티티 position과 버퍼의 마지막 position과의 거리가 일정 거리 이하일 때 or 버퍼의 마지막 position을 목적지로 삼았을때
//    PathLazyRefineRequirement를 컴포넌트에 추가 (최적화적으로 안좋은 방법인거 같긴함)

// PathfindingBridge Lazy Refine
// 1. HighLevelWaypoint 버퍼에서 from, to 받음
// 2. HighLevelClusterPath 버퍼에서 clusterIndex들 받음 (찾는 방식은 개선이 필요할 듯)
// 3. searchWithTheClusterResult.FindPathThetaWithClusterList로 low level 경로 탐색
// 4. 기존 LowLevelWaypoint 버퍼 제거, 경로 추가