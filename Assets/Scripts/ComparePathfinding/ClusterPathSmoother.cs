using UnityEngine;
using System.Collections.Generic;

public class ClusterPathSmoother
{
    private readonly PathManager pathManager;
    private readonly HPAClusterList clusterList;
    private readonly NodeList nodeList;

    private readonly List<Vector2Int> clusterIndexes = new();
    private readonly List<ClusterResult> smootherClusterPath = new();

    public ClusterPathSmoother(HPAClusterList clusterList, NodeList nodeList, PathManager pathManager)
    {
        this.clusterList = clusterList;
        this.nodeList = nodeList;
        this.pathManager = pathManager;
    }

    public List<ClusterResult> SmoothClusterPath(List<ClusterResult> clusterPathList)
    {
        if (clusterPathList == null || clusterPathList.Count <= 1) return null;
        
        float unitRadius = clusterPathList[0].UnitRadius;
        
        clusterIndexes.Clear();
        smootherClusterPath.Clear();
        int leftSetIndex = 0, rightSetIndex = 0;

        Vector3 from = pathManager.From;
        Vector3 to = pathManager.To;

        Vector2Int startPoint = nodeList.GetNodeIndex(from);

        for (int index = 0; index < clusterPathList.Count - 1;)
        {
            Loop(clusterList, nodeList, clusterPathList, from, Vector2Int.zero, leftSetIndex, Vector2Int.zero, rightSetIndex, out Vector3 outPoint, out int outIndex, index, true, unitRadius);
            from = outPoint;
            index = outIndex + 1;
            leftSetIndex = 0;
            rightSetIndex = 0;            

            if (index < clusterPathList.Count)
            {
                var clusterPath = clusterPathList[index].GetClusterResult();
                
                if (clusterIndexes.Contains(clusterPath.Index))
                {
                    SetResult(nodeList.GetNodeIndex(from), startPoint, Vector2Int.zero, false);
                }
                else
                {
                    SetResult(nodeList.GetNodeIndex(from), startPoint, clusterPath.Index, true);
                }
                clusterIndexes.Clear();
            }
            else
            {
                // 마지막 노드 세팅
                clusterIndexes.Add(clusterPathList[^1].GetClusterResult().Index);
                SetResult(nodeList.GetNodeIndex(to), startPoint, Vector2Int.zero, false);
            }
        }

        PathResultRecorder.AddMemoryUsed(clusterIndexes.Count);
        return smootherClusterPath;
    }

    private void Loop(HPAClusterList clusterList, NodeList nodeList, List<ClusterResult> clusterPath,
        Vector3 point, Vector2Int currentLeft, int leftSetIndex, Vector2Int currentRight, int rightSetIndex,
        out Vector3 outPoint, out int outIndex, int index, bool isStart, float unitRadius)
    {
        outPoint = point;
        outIndex = index;
        PathResultRecorder.AddSearchedCount();

        if (index == 0)
        {
            smootherClusterPath.Clear();
        }
        else if (index >= clusterPath.Count - 1)
        {
            return;
        }

        var path = clusterPath[index].GetClusterResult();

        if (isStart)
        {
            clusterList.GetCluster(path.Index).Graph.GetUsedEntrance(path.ExitDirection, path.EntranceExit, out Vector2Int left, out Vector2Int right, unitRadius);
            currentLeft = left;
            leftSetIndex = index;
            currentRight = right;
            rightSetIndex = index;
            clusterIndexes.Add(path.Index);
            Loop(clusterList, nodeList, clusterPath, point, currentLeft, leftSetIndex, currentRight, rightSetIndex, out outPoint, out outIndex, index + 1, false, unitRadius);
            return;
        }

        var currentLeftString = (Vector3)nodeList.GridToWorld(currentLeft) - point;
        var currentRightString = (Vector3)nodeList.GridToWorld(currentRight) - point;
        float angle = Vector3.SignedAngle(currentLeftString, currentRightString, Vector3.forward);
        int angleSign = angle > 0 ? 1 : -1;

        if (angle == 0 && currentLeftString.normalized == currentRightString.normalized)
        {
            // point에 더 가까운 쪽으로 새 point를 결정
            Vector3 addString = currentLeftString.sqrMagnitude < currentRightString.sqrMagnitude ? currentLeftString : currentRightString;
            outPoint = point + addString;
            outIndex = index - 1;
            return;
        }

        clusterList.GetCluster(path.Index).Graph.GetUsedEntrance(path.ExitDirection, path.EntranceExit, out Vector2Int newLeft, out Vector2Int newRight, unitRadius);

        // 왼쪽 endPoint 계산
        Vector3 newLeftString = (Vector3)nodeList.GridToWorld(newLeft) - point;
        float newAngle = Vector3.SignedAngle(newLeftString, currentRightString, Vector3.forward);
        int newAngleSign = newAngle > 0 ? 1 : -1;

        if (angleSign * newAngleSign > 0 && Mathf.Abs(newAngle) <= Mathf.Abs(angle))
        {
            // 각도가 더 줄어드는 방향이면 leftEndPoint 갱신
            currentLeft = newLeft;
            currentLeftString = newLeftString;
            leftSetIndex = index;
            angle = newAngle;
            angleSign = newAngleSign;
        }
        else if (angleSign * newAngleSign < 0)
        {
            // right 선을 지나가면 point 갱신, 리턴
            outPoint = point + currentRightString;
            outIndex = rightSetIndex;
            return;
        }
        // 각도가 더 커지는 방향이면 아무것도 하지 않음

        // 오른쪽 endPoint 계산
        Vector3 newRightString = (Vector3)nodeList.GridToWorld(newRight) - point;
        newAngle = Vector3.SignedAngle(currentLeftString, newRightString, Vector3.forward);
        newAngleSign = newAngle > 0 ? 1 : -1;

        if (angleSign * newAngleSign > 0 && Mathf.Abs(newAngle) <= Mathf.Abs(angle))
        {
            // 각도가 더 줄어드는 방향이면 rightEndPoint 갱신
            currentRight = newRight;
            rightSetIndex = index;
        }
        else if (angleSign * newAngleSign < 0)
        {
            // left 선을 지나가면 point 갱신, 리턴
            outPoint = point + currentLeftString;
            outIndex = leftSetIndex;
            return;
        }
        // 각도가 더 커지는 방향이면 아무것도 하지 않음

        clusterIndexes.Add(path.Index);

        Loop(clusterList, nodeList, clusterPath, point, currentLeft, leftSetIndex, currentRight, rightSetIndex, out outPoint, out outIndex, index + 1, false, unitRadius);
    }

    private void SetResult(Vector2Int nodeIndex, Vector2Int from, Vector2Int notIncludeClusterIndex, bool useLastIncludeClusterIndex)
    {
        Vector2Int start;

        if (clusterIndexes.Count == 0) return;

        if (smootherClusterPath.Count > 0)
        {
            var path = smootherClusterPath[^1].GetSmoothClusterPath();

            Vector2Int dir = clusterIndexes[0] - path.ClusterIndexes[^1];
            start = path.ExitNodeIndex + dir;
        }
        else
        {
            start = from;
        }

        ClusterResult result = new();
        result.SetSmootherPath(clusterIndexes, nodeIndex, start, notIncludeClusterIndex, useLastIncludeClusterIndex);

        smootherClusterPath.Add(result);
    }
}

// public class ClusterSmootherResult
// {
//     public List<Vector2Int> ClusterIndexes = new();
//     public Vector2Int EnterNodeIndex;
//     public Vector2Int ExitNodeIndex;

//     public void SetSmootherResult(List<Vector2Int> clusters, Vector2Int exitIndex, Vector2Int startIndex, Vector2Int notIncludeClusterIndex, bool useNotIncludeClusterIndex)
//     {
//         ClusterIndexes.Clear();
//         for (int i = 0; i < clusters.Count; i++)
//         {
//             if (useNotIncludeClusterIndex && clusters[i] == notIncludeClusterIndex) continue;

//             ClusterIndexes.Add(clusters[i]);
//         }
//         EnterNodeIndex = startIndex;
//         ExitNodeIndex = exitIndex;
//     }
// }
