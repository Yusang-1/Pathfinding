using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class UnitController
    {
        private readonly Unit unit;
        private readonly UnitSO unitData;
        private readonly LazyRefine lazyRefine;
        private readonly Transform bottomChangerTransform;
        private readonly UnitRuntimeContext unitRuntimeContext;
        private SteeringConfig steeringConfig;


        private List<ClusterSmootherResult> abstractPath;
        private int currentPathIndex;
        private bool isMoving;
        private Vector3 startPosition;
        private Vector3 finalDestination;
        private Vector3 shortDestination;
        private Vector3 exitOfCluster;
        private Vector3 velocity;

        public Vector3 Velocity => velocity;

        public UnitController(Unit unit, UnitRuntimeContext unitRuntimeContext, Transform bottomChangerTransform, SteeringConfig steeringConfig, UnitSO unitData)
        {
            this.unit = unit;
            this.unitRuntimeContext = unitRuntimeContext;
            this.bottomChangerTransform = bottomChangerTransform;
            this.unitData = unitData;
            this.steeringConfig = steeringConfig;
            lazyRefine = unitRuntimeContext.Pathfinder.GetLazyRefine();

            unitRuntimeContext.SpatialHash.AddUnit(unit);
        }
        
        private int totalUnitCount;
        public void MoveTo(Vector3 destination, int totalUnitCount)
        {
            this.totalUnitCount = totalUnitCount;
            
            if (isMoving)
            {
                lazyRefine.ResetLazyRefine();
            }

            startPosition = unit.transform.position;
            finalDestination = destination;
            currentPathIndex = 0;
            abstractPath = unitRuntimeContext.Pathfinder.GetAbstractPath(unit.transform.position, destination, unitData.Radius);
            if (abstractPath == null || abstractPath.Count == 0) return;

            SearchLowLevelPath(abstractPath[currentPathIndex], abstractPath.Count == 1, true);
            TryGetShortDestination(out shortDestination); // 출발지(현재 위치) 빼내기
            GetShortDestination();
            isMoving = true;
        }

        public void MoveToReservation(Vector3 destination, int totalUnitCount)
        {
            this.totalUnitCount = totalUnitCount;
            
            bool haveToDoLazyRefine = false;
            if (currentPathIndex + 1 == abstractPath.Count) haveToDoLazyRefine = true;

            var newAbstractPath = unitRuntimeContext.Pathfinder.GetAbstractPath(finalDestination, destination, unitData.Radius);
            if (abstractPath == null || abstractPath.Count == 0) return;

            startPosition = finalDestination;
            abstractPath.AddRange(newAbstractPath);
            finalDestination = destination;
            if (haveToDoLazyRefine)
            {
                SearchLowLevelPath(abstractPath[++currentPathIndex], currentPathIndex == abstractPath.Count - 1, true);
            }
        }

        public void ControllerUpdate()
        {
            Move();
        }

        public void ControllerLateUpdate()
        {
            bottomChangerTransform.position = unit.transform.position;
        }

        private void Move()
        {
            if (!isMoving) return;

            GetVelocity();
            unit.transform.position += velocity;

            unitRuntimeContext.SpatialHash.CheckUnitHash(unit);

            if (IsDistanceInCurrentDestination())
            {
                GetShortDestination();
            }
        }

        private void GetShortDestination()
        {
            bool isSuccess = TryGetShortDestination(out shortDestination);
            if (isSuccess)
            {
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
                unit.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                velocity = Vector3.zero;
            }
        }

        private bool IsDistanceInCurrentDestination()
        {
            float sqrtM = Vector3.SqrMagnitude(unit.transform.position - shortDestination);
            if (sqrtM <= 0.5f)
            {
                if (shortDestination == finalDestination) // 최종 도착
                {
                    velocity = Vector3.zero;
                    isMoving = false;
                    return false;
                }

                unit.transform.position = shortDestination;
                return true;
            }
            else return false;
        }

        private bool TryGetShortDestination(out Vector3 path)
        {
            if (lazyRefine.TryGetPathFromQueue(out path))
            {
                // 받은 경로가 cluster의 exit인 경우 다음 cluster의 lowLevelPath를 찾음
                if (path == exitOfCluster && currentPathIndex + 1 < abstractPath.Count)
                {
                    bool isEnd = false;
                    if (currentPathIndex + 1 == abstractPath.Count - 1) isEnd = true;

                    SearchLowLevelPath(abstractPath[++currentPathIndex], isEnd, false);
                }

                return true;
            }
            else return false;
        }

        private void SearchLowLevelPath(ClusterSmootherResult resultNode, bool isEnd, bool isStart)
        {
            lazyRefine.DoLazyRefinement(resultNode, isEnd, finalDestination, isStart, startPosition, unitData.Radius);

            exitOfCluster = new Vector3(abstractPath[currentPathIndex].ExitNodeIndex.x, abstractPath[currentPathIndex].ExitNodeIndex.y);
        }

        private void GetVelocity()
        {
            var nearbyUnits = unitRuntimeContext.SpatialHash.GetUnitsInRange(unit.transform.position, 2.2f);
            velocity = unitRuntimeContext.SteeringBehavior.GetSteering(unit, nearbyUnits, unitData.MoveSpeed, shortDestination, steeringConfig);
            velocity *= Time.deltaTime;
        }
    }
}
