using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class UnitController : MonoBehaviour
    {
        private Pathfinder pathfinder;
        private Vector3 direction;
        private bool HasDirection => direction != Vector3.zero;
        private bool isReadyToNextPathSet;

        [SerializeField] private UnitInput input;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float refineLength = 2.2f;

        private void Start()
        {
            pathfinder = FindAnyObjectByType<Pathfinder>();
        }

        private List<HPAPathfinder.ResultNode> abstractPath;
        private HPAPathfinder.ResultNode currentAbstractPath;
        private int currentPathIndex;
        private Vector3 shortDestination;
        private Vector3 exitOfCluster;
        
        public void MoveTo(Vector3 destination)
        {
            isReadyToNextPathSet = true;
            currentPathIndex = 0;
            abstractPath = pathfinder.GetAbstractPath(transform.position, destination);
            exitOfCluster = new Vector3(abstractPath[currentPathIndex].exitNode.x, abstractPath[currentPathIndex].exitNode.y);
            pathfinder.SearchLowLevelPath(abstractPath[currentPathIndex]);
            // currentAbstractPath = abstractPath[currentPathIndex];
            pathfinder.TryGetShortDestination(out shortDestination); // 출발지(현재 위치) 빼내기
            GetShortDestination();
        }

        public void ControllerUpdate()
        {
            Move();
        }

        private void Move()
        {
            if (!HasDirection) return;

            transform.position += moveSpeed * Time.deltaTime * direction;

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
                direction = (shortDestination - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, angle);

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
            float sqrM = Vector3.SqrMagnitude(transform.position - exitOfCluster);
            float compare = refineLength * refineLength;
            if (sqrM <= compare) return true;
            else return false;
        }

        private bool IsDistanceInCurrentDestination()
        {
            float sqrtM = Vector3.SqrMagnitude(transform.position - shortDestination);
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

                transform.position = shortDestination;
                return true;
            }
            else return false;
        }
    }
}
