using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class UnitController
    {
        private readonly Unit unit;
        private readonly UnitSO unitData;
        private readonly Pathfinder pathfinder;
        private readonly LazyRefine lazyRefine;
        private readonly SpatialHash spatialHash;
        private readonly Transform bottomChangerTransform;
        // private readonly SteeringBehavior steeringBehavior = new();

        private Vector3 direction;
        private bool HasDirection => direction != Vector3.zero;

        private List<HPAPathfinder.ResultNode> abstractPath;
        private int currentPathIndex;
        private Vector3 shortDestination;
        private Vector3 exitOfCluster;

        public UnitController(Unit unit, SpatialHash spatialHash, Transform bottomChangerTransform, UnitSO unitData, Pathfinder pathfinder)
        {
            this.unit = unit;
            this.spatialHash = spatialHash;
            this.bottomChangerTransform = bottomChangerTransform;
            this.unitData = unitData;
            this.pathfinder = pathfinder;
            lazyRefine = pathfinder.GetLazyRefine();

            spatialHash.AddUnit(unit);
        }

        public void MoveTo(Vector3 destination)
        {
            currentPathIndex = 0;
            abstractPath = pathfinder.GetAbstractPath(unit.transform.position, destination);
            if (abstractPath == null || abstractPath.Count == 0) Debug.Log(22);
            SearchLowLevelPath(abstractPath[currentPathIndex]);
            TryGetShortDestination(out shortDestination); // 출발지(현재 위치) 빼내기
            GetShortDestination();
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
            if (!HasDirection) return;

            unit.transform.position += unitData.MoveSpeed * Time.deltaTime * direction;
            spatialHash.CheckUnitHash(unit);

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
                direction = (shortDestination - unit.transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                unit.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                direction = Vector3.zero;
            }
        }

        private bool IsDistanceInCurrentDestination()
        {
            float sqrtM = Vector3.SqrMagnitude(unit.transform.position - shortDestination);
            if (sqrtM <= 0.1f)
            {
                if (shortDestination == exitOfCluster && currentPathIndex >= abstractPath.Count) // 최종 도착
                {
                    direction = Vector3.zero;
                    return false;
                }

                unit.transform.position = shortDestination;
                return true;
            }
            else return false;
        }

        public void SearchLowLevelPath(HPAPathfinder.ResultNode resultNode)
        {
            lazyRefine.DoLazyRefinement(resultNode);
            exitOfCluster = new Vector3(abstractPath[currentPathIndex].exitNode.x, abstractPath[currentPathIndex].exitNode.y);
        }

        public bool TryGetShortDestination(out Vector3 path)
        {
            if (lazyRefine.TryGetPathFromQueue(out path))
            {
                // 받은 경로가 cluster의 exit인 경우 다음 cluster의 lowLevelPath를 찾음
                if (path == exitOfCluster && currentPathIndex + 1 < abstractPath.Count)
                {
                    SearchLowLevelPath(abstractPath[++currentPathIndex]);
                }

                return true;
            }
            else return false;
        }
    }
}
