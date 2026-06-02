using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private LazyRefine lazyRefine;
    [SerializeField] private float unitSpeed = 3.2f;
    [SerializeField] private float refineLength = 2.2f;

    private Vector3 curDestination;
    private Vector3 direction;
    private bool HasDestination => direction != Vector3.zero;

    private HPAPathfinder.ResultNode currentPath;
    private Vector3 lazyGoal;

    private List<HPAPathfinder.ResultNode> currentAbstractPath;
    private int pathNum;

    private LineDrawer lineDrawer;

    private void Update()
    {
        Move();
    }

    public void Initialize(LineDrawer lineDrawer)
    {
        this.lineDrawer = lineDrawer;
    }

    private void Move()
    {
        if (!HasDestination) return;
        transform.position += Time.deltaTime * unitSpeed * direction;

        if (!isNextPathSet && CheckLengthOfNextRefine())
        {
            if (pathNum < currentAbstractPath.Count)
            {
                // 다음 lazyRefinement시작
                lazyRefine.DoLazyRefinement(currentAbstractPath[pathNum], lineDrawer);
                isNextPathSet = true;
            }
        }

        if (IsUnitArriveAtCurrentDestination()) // 단기 목적지에 도달한 경우
        {
            if (lazyRefine.TryGetPathFromQueue(out Vector3 destination))
            {
                curDestination = destination;
                direction = (curDestination - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                direction = Vector3.zero;
                return;
            }
        }
    }

    public void MoveWithResult(List<HPAPathfinder.ResultNode> resultNodes, HPAPathfinder hPAPathfinder, ThetaStar thetaStar, HPAClusterList clusterList, NodeList nodeList, SearchWithTheClusterResult searchWithTheClusterResult)
    {
        gameObject.SetActive(true);
        // transform.position = new Vector3(resultNodes[0].enteranceNode.x, resultNodes[0].enteranceNode.y, 0);

        pathNum = 0;
        currentPath = resultNodes[pathNum];
        lazyGoal = new Vector3(currentPath.exitNode.x, currentPath.exitNode.y);
        currentAbstractPath = resultNodes;

        lazyRefine ??= new LazyRefine(hPAPathfinder, thetaStar, clusterList, nodeList, searchWithTheClusterResult);
        lazyRefine.DoLazyRefinement(resultNodes[pathNum++], lineDrawer);

        if (lazyRefine.TryGetPathFromQueue(out Vector3 destination))
        {
            transform.position = destination;
        }
        else
        {
            direction = Vector3.zero;
            return;
        }

        if (lazyRefine.TryGetPathFromQueue(out destination))
        {
            curDestination = destination;
            direction = (curDestination - transform.position).normalized;
            transform.rotation = Quaternion.AngleAxis(-Vector3.Angle(Vector3.up, curDestination - transform.position), Vector3.forward);
        }
        else
        {
            direction = Vector3.zero;
            return;
        }
    }

    private bool CheckLengthOfNextRefine()
    {
        float sqrM = Vector3.SqrMagnitude(transform.position - lazyGoal);
        float compare = refineLength * refineLength;
        if (sqrM <= compare) return true;
        else return false;
    }

    private bool isNextPathSet;
    private bool IsUnitArriveAtCurrentDestination()
    {
        float sqrtM = Vector3.SqrMagnitude(transform.position - curDestination);
        if (sqrtM <= 0.05f)
        {
            if (curDestination == new Vector3(currentPath.exitNode.x, currentPath.exitNode.y))
            {
                if (pathNum >= currentAbstractPath.Count) // 최종 도착
                {
                    direction = Vector3.zero;
                    return false;
                }

                currentPath = currentAbstractPath[pathNum++];
                lazyGoal = new Vector3(currentPath.exitNode.x, currentPath.exitNode.y);
                isNextPathSet = false;
            }

            transform.position = curDestination;
            return true;
        }
        else return false;
    }
}
