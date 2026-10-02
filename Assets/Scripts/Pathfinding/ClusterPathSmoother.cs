using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.Pathfinding
{
    public class ClusterPathSmoother
    {
        private readonly NodeList nodeList;
        private readonly HPAClusterList clusterList;

        private readonly List<Vector2Int> clusterIndexes = new();

        public ClusterPathSmoother(NodeList nodeList, HPAClusterList clusterList)
        {
            this.nodeList = nodeList;
            this.clusterList = clusterList;
        }

        public ClusterResultWrapper SmoothClusterPath(ClusterResultWrapper wrapper)
        {
            clusterIndexes.Clear();

            List<ClusterResult> clusterPath = wrapper.ClusterResults;
            Vector3 from = wrapper.From;
            Vector3 to = wrapper.To;
            float unitRadius = wrapper.UnitRadius;

            if (clusterPath == null || clusterPath.Count < 1) return null;
            else if (clusterPath.Count == 1)
            {
                clusterIndexes.Add(clusterPath[0].Index);
                var smootherResult = ClusterSmootherResultPool.GetValue();
                smootherResult.SetSmootherResult(clusterIndexes, nodeList.GetNodeIndex(to), nodeList.GetNodeIndex(from), Vector2Int.zero, false);

                wrapper.SetClusterSmootherResult(smootherResult);

                return wrapper;
            }

            int leftSetIndex = 0, rightSetIndex = 0;
            Vector2Int startPoint = nodeList.GetNodeIndex(from);

            List<ClusterSmootherResult> smootherList = wrapper.ClusterSmootherResult;

            for (int index = 0; index < clusterPath.Count - 1;)
            {
                Loop(clusterList, nodeList, clusterPath, from, Vector2Int.zero, leftSetIndex, Vector2Int.zero, rightSetIndex,
                    out Vector3 outPoint, out int outIndex, index, true, unitRadius);
                from = outPoint;
                index = outIndex + 1;
                leftSetIndex = 0;
                rightSetIndex = 0;

                if (index < clusterPath.Count)
                {
                    if (clusterIndexes.Contains(clusterPath[index].Index))
                    {
                        SetResult(nodeList.GetNodeIndex(from), startPoint, Vector2Int.zero, false, smootherList);
                    }
                    else
                    {
                        SetResult(nodeList.GetNodeIndex(from), startPoint, clusterPath[index].Index, true, smootherList);
                    }
                    clusterIndexes.Clear();
                }
                else
                {
                    // 마지막 노드 세팅
                    clusterIndexes.Add(clusterPath[^1].Index);
                    SetResult(nodeList.GetNodeIndex(to), startPoint, Vector2Int.zero, false, smootherList);
                }
            }

            PathResultRecorder.AddMemoryUsed(clusterIndexes.Count);

            return wrapper;
        }

        private void Loop(HPAClusterList clusterList, NodeList nodeList, List<ClusterResult> clusterPath,
            Vector3 point, Vector2Int currentLeft, int leftSetIndex, Vector2Int currentRight, int rightSetIndex,
            out Vector3 outPoint, out int outIndex, int index, bool isStart, float unitRadius)
        {
            outPoint = point;
            outIndex = index;
            PathResultRecorder.AddSearchedCount();

            if (index >= clusterPath.Count - 1)
            {
                return;
            }

            var path = clusterPath[index];

            // loop의 첫 시작인 경우 left, right설정 후 다음 loop로
            if (isStart)
            {
                clusterList.GetCluster(path.Index).Graph
                    .GetUsedEntrance(path.ExitDirection, path.EntranceExit, out Vector2Int left, out Vector2Int right, unitRadius);

                // 현재 path의 EntranceExit이 cluster의 entrance영역 위에 있다면 이 path는 포함시키고 바로 다음 loop로 넘어감
                if (clusterList.IsNodeInEntrance(path.Index, path.EntranceExit, path.ExitDirection, unitRadius))
                {
                    clusterIndexes.Add(path.Index);

                    Loop(clusterList, nodeList, clusterPath, point, Vector2Int.zero, leftSetIndex, Vector2Int.zero, rightSetIndex,
                    out outPoint, out outIndex, index + 1, true, unitRadius);
                    return;
                }

                currentLeft = left;
                leftSetIndex = index;
                currentRight = right;
                rightSetIndex = index;
                clusterIndexes.Add(path.Index);

                Loop(clusterList, nodeList, clusterPath,
                    point, currentLeft, leftSetIndex, currentRight, rightSetIndex,
                    out outPoint, out outIndex, index + 1, false, unitRadius);

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

            clusterList.GetCluster(path.Index).Graph.
                GetUsedEntrance(path.ExitDirection, path.EntranceExit, out Vector2Int newLeft, out Vector2Int newRight, unitRadius);

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

                // outPoint가 path.Index인 cluster에 있다면
                if (clusterList.IsNodeInCluster(path.Index, nodeList.GetNodeIndex(outPoint)))
                {
                    clusterIndexes.Add(path.Index);
                }
                else // 없다면 outIndex--, clusterIndexes에 넣지 않기
                {
                    outIndex--;
                }

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

                if (clusterList.IsNodeInCluster(path.Index, nodeList.GetNodeIndex(outPoint)))
                {
                    clusterIndexes.Add(path.Index);
                }
                else
                {
                    outIndex--;
                }

                return;
            }
            // 각도가 더 커지는 방향이면 아무것도 하지 않음

            clusterIndexes.Add(path.Index);

            Loop(clusterList, nodeList, clusterPath, point, currentLeft, leftSetIndex, currentRight, rightSetIndex,
                out outPoint, out outIndex, index + 1, false, unitRadius);
        }

        private void SetResult(Vector2Int nodeIndex, Vector2Int from, Vector2Int notIncludeClusterIndex, bool useLastIncludeClusterIndex,
            List<ClusterSmootherResult> smootherList)
        {
            Vector2Int start;

            if (clusterIndexes.Count == 0) return;

            if (smootherList.Count > 0)
            {
                Vector2Int dir = clusterIndexes[0] - smootherList[^1].ClusterIndexes[^1];
                start = smootherList[^1].ExitNodeIndex + dir;
            }
            else
            {
                start = from;
            }

            ClusterSmootherResult result = ClusterSmootherResultPool.GetValue();
            result.SetSmootherResult(clusterIndexes, nodeIndex, start, notIncludeClusterIndex, useLastIncludeClusterIndex);

            smootherList.Add(result);
        }
    }
}
