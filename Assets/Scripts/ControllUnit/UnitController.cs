using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class UnitController
    {
        private readonly Pathfinder pathfinder;
        private readonly SpatialHash spatialHash;
        private readonly Unit unit;
        private readonly UnitSO unitData;
        private readonly Transform bottomChangerTransform;

        private Vector3 direction;
        private bool HasDirection => direction != Vector3.zero;
        private bool isReadyToNextPathSet;        

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
            
            spatialHash.AddUnit(unit);
        }

        public void MoveTo(Vector3 destination)
        {
            isReadyToNextPathSet = true;
            currentPathIndex = 0;
            abstractPath = pathfinder.GetAbstractPath(unit.transform.position, destination);
            exitOfCluster = new Vector3(abstractPath[currentPathIndex].exitNode.x, abstractPath[currentPathIndex].exitNode.y);
            pathfinder.SearchLowLevelPath(abstractPath[currentPathIndex]);
            pathfinder.TryGetShortDestination(out shortDestination); // 출발지(현재 위치) 빼내기
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

            if (isReadyToNextPathSet && IsDistanceInNextRefine() && currentPathIndex < abstractPath.Count)
            {
                if (currentPathIndex + 1 < abstractPath.Count)
                {
                    pathfinder.SearchLowLevelPath(abstractPath[++currentPathIndex]);
                    isReadyToNextPathSet = false;
                }
            }

            if (IsDistanceInCurrentDestination())
            {
                GetShortDestination();
            }
        }

        private void GetShortDestination()
        {
            bool isSuccess = pathfinder.TryGetShortDestination(out shortDestination);
            if (isSuccess)
            {
                direction = (shortDestination - unit.transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                unit.transform.rotation = Quaternion.Euler(0, 0, angle);

                if (shortDestination == exitOfCluster)
                {
                    isReadyToNextPathSet = true;
                }
            }
            else
            {
                direction = Vector3.zero;
            }
        }

        private bool IsDistanceInNextRefine()
        {
            float sqrM = Vector3.SqrMagnitude(unit.transform.position - exitOfCluster);
            float compare = unitData.RefineLength * unitData.RefineLength;
            if (sqrM <= compare) return true;
            else return false;
        }

        private bool IsDistanceInCurrentDestination()
        {
            float sqrtM = Vector3.SqrMagnitude(unit.transform.position - shortDestination);
            if (sqrtM <= 0.1f)
            {
                if (shortDestination == exitOfCluster)
                {
                    if (currentPathIndex >= abstractPath.Count) // 최종 도착
                    {
                        direction = Vector3.zero;
                        return false;
                    }

                    exitOfCluster = new Vector3(abstractPath[currentPathIndex].exitNode.x, abstractPath[currentPathIndex].exitNode.y);
                }

                unit.transform.position = shortDestination;
                return true;
            }
            else return false;
        }
    }
}
