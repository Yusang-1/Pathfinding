using UnityEngine;
using System.Collections.Generic;
using System;

namespace Assets.Scripts.ControllUnit
{
    public class AStarPathfinder : AbstractPathfinder
    {
        private readonly NodeList nodeList;
        private readonly HPAClusterList hPAClusterList;

        private readonly PriorityQueue<Vector2Int, float> openList = new();
        private readonly HashSet<Vector2Int> closeList = new();
        private readonly Dictionary<Vector2Int, PathNode> nodeDict = new();

        public AStarPathfinder(NodeList nodeList, HPAClusterList hPAClusterList)
        {
            this.nodeList = nodeList;
            this.hPAClusterList = hPAClusterList;
            directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        }

        public float FindPathInClusterForPathCache(Vector2Int from, Vector2Int to, float unitRadius)
        {
            Vector3 fromPos = nodeList.GridToWorld(from);
            Vector3 toPos = nodeList.GridToWorld(to);

            List<Vector3> path = SearchPath(fromPos, toPos, unitRadius, GetNeighborNodesInCluster);

            float pathLength;
            if (path != null)
            {
                pathLength = nodeDict[to].g;
            }
            else
            {
                pathLength = 0;
            }

            Vector3ListPool.ReleaseValue(path);
            return pathLength;
        }

        protected override List<Vector3> SearchPath(Vector3 startPosition, Vector3 destinationPosition, float unitRadius, Func<Vector2Int, float, List<Vector2Int>> getNeighbors)
        {
            openList.Clear();
            closeList.Clear();
            nodeDict.Clear();

            Vector2Int startIndex = nodeList.GetNodeIndex(startPosition);
            Vector2Int goalIndex = nodeList.GetNodeIndex(destinationPosition);

            if (!nodeList.IsNodeAccessable(startIndex, goalIndex))
            {
                Debug.Log("접근 불가능한 노드입니다.");
                return null;
            }

            PathNode startNode = new PathNode
            {
                index = startIndex
            };
            openList.Enqueue(startNode.index, startNode.f);
            nodeDict[startNode.index] = startNode;

            while (openList.Count > 0)
            {
                Vector2Int current = openList.Dequeue();

                if (current == goalIndex)
                {
                    return CaculateResult(nodeDict, current, startIndex);
                }

                closeList.Add(current);

                List<Vector2Int> neighborList = getNeighbors(current, unitRadius);
                for (int i = 0; i < neighborList.Count; i++)
                {
                    Vector2Int neighbor = neighborList[i];

                    if (closeList.Contains(neighbor)) continue;

                    float moveCost = GetMoveCost(current, neighbor);
                    float newG = nodeDict[current].g + moveCost;

                    if (!nodeDict.ContainsKey(neighbor) || newG < nodeDict[neighbor].g)
                    {
                        if (!nodeDict.ContainsKey(neighbor))
                        {
                            nodeDict[neighbor] = new PathNode
                            {
                                index = neighbor,
                                h = CaculateHeuristic(neighbor, goalIndex)
                            };
                        }
                        PathNode neighborNode = nodeDict[neighbor];
                        neighborNode.g = newG;
                        neighborNode.parentIndex = current;
                        neighborNode.isParentSet = true;
                        nodeDict[neighbor] = neighborNode;

                        openList.Enqueue(nodeDict[neighbor].index, nodeDict[neighbor].f);
                    }
                }
                Vector2IntListPool.ReleaseValue(neighborList);
            }

            // 경로 찾지 못함
            return null;
        }

        private readonly Vector2Int[] directions;
        private List<Vector2Int> GetNeighborNodesInCluster(Vector2Int current, float unitRadius)
        {
            List<Vector2Int> neighbors = Vector2IntListPool.GetValue();

            // 상하좌우
            for (int i = 0; i < directions.Length; i++)
            {
                int newX = current.x + directions[i].x;
                int newY = current.y + directions[i].y;

                Vector2Int neighbor = new(newX, newY);

                if (newX < 0 || newY < 0
                    || newX >= nodeList.Nodes.GetLength(0) || newY >= nodeList.Nodes.GetLength(0)
                    || !hPAClusterList.IsNodeInCluster(hPAClusterList.GetClusterIndex(current), neighbor))
                {
                    continue;
                }

                // 워크어빌리티 맵으로 확인      
                if (CanUnitFitAtNode(neighbor, unitRadius))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        private bool CanUnitFitAtNode(Vector2Int nodeIndex, float unitRadius)
        {
            bool result = true;

            List<Node> nodeInRadius = nodeList.GetNodesInRange(nodeIndex, unitRadius);
            foreach (var node in nodeInRadius)
            {
                if (!result) break;

                result = result && node.IsWalkable;
            }

            return result;
        }

        private List<Vector3> CaculateResult(Dictionary<Vector2Int, PathNode> nodes, Vector2Int current, Vector2Int start)
        {
            var path = Vector2IntListPool.GetValue();

            while (current != start)
            {
                path.Add(current);
                if (!nodes[current].isParentSet)
                    break;
                current = nodes[current].parentIndex;
            }
            path.Add(start);
            path.Reverse();

            // 그리드 좌표를 월드 좌표로 변환
            var worldPath = Vector3ListPool.GetValue();
            foreach (var gridPos in path)
            {
                worldPath.Add(nodeList.GridToWorld(gridPos));
            }
            Vector2IntListPool.ReleaseValue(path);

            return worldPath;
        }

        protected override float CaculateHeuristic(Vector2Int from, Vector2Int to)
        {
            int dx = Mathf.Abs(to.x - from.x);
            int dy = Mathf.Abs(to.y - from.y);

            const float ORTHOGONAL_COST = 1f;
            const float DIAGONAL_COST = 1.4142f;
            // 대각선으로 이동 가능한 최대 거리 + 남은 수평/수직 거리        
            return (Mathf.Min(dx, dy) * DIAGONAL_COST) + (Mathf.Abs(dx - dy) * ORTHOGONAL_COST);
        }

        private float GetMoveCost(Vector2Int from, Vector2Int to) => from.GetNeighborMoveCost(to);
    }
}
