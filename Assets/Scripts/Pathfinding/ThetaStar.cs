using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Pathfinding
{
    public class ThetaStar : AbstractPathfinder
    {
        private readonly NodeList nodeList;
        private const float EPS = 1e-4f;

        private readonly PriorityQueue<Vector2Int, float> openList = new();
        private readonly HashSet<Vector2Int> closeList = new();
        private readonly Dictionary<Vector2Int, PathNode> nodeDict = new();

        public IGetNeighborNodesActionProvider GetNeighborNodesActionProvider { get; private set; }

        public ThetaStar(NodeList nodeList)
        {
            this.nodeList = nodeList;
        }

        public override List<Vector3> FindPath(Vector3 from, Vector3 to, float unitRadius)
        {
            nodeList.NodeTypeController.SetNodeTypeInPathFinding(nodeList.GetNodeIndex(from), NodeType.trace);
            var result = SearchThetaStar(from, to, unitRadius, GetNeighborNodesActionProvider.GetNeighborNodes);
            nodeList.NodeTypeController.SetNodeTypeInPathFinding(nodeList.GetNodeIndex(to), NodeType.trace);

            return result;
        }

        // public List<Vector3> FindPathInClusterList(Vector3 from, Vector3 to, List<Vector2Int> clusters)
        // {
        //     neighborFindingClusters = clusters;
        //     return SearchThetaStar(from, to, GetNeighborNodesActionProvider.GetNeighborNodes);
        // }

        public void SetGetNeighborPolicy(IGetNeighborNodesActionProvider policyProvider)
        {
            GetNeighborNodesActionProvider = policyProvider;
        }

        private List<Vector3> SearchThetaStar(Vector3 from, Vector3 to, float unitRadius, Func<Vector2Int, float, List<Vector2Int>> getNeighborNodeFunc)
        {
            openList.Clear();
            closeList.Clear();
            nodeDict.Clear();

            Vector2Int startNodeIndex = nodeList.GetNodeIndex(from);
            Vector2Int goalNodeIndex = nodeList.GetNodeIndex(to);

            var startNode = new PathNode
            {
                index = startNodeIndex,
                g = 0
            };
            openList.Enqueue(startNode.index, startNode.f);
            nodeDict.Add(startNode.index, startNode);

            while (openList.Count > 0)
            {
                Vector2Int currentIndex = openList.Dequeue();
                closeList.Add(currentIndex);

                if (currentIndex == goalNodeIndex)
                {
                    PathResultRecorder.AddPathLength(nodeDict[currentIndex].g);
                    PathResultRecorder.AddMemoryUsed(openList.Capacity + closeList.Count + nodeDict.Count);

                    return CaculateResult(nodeDict, currentIndex, startNodeIndex);
                }

                List<Vector2Int> neighborIndexes = getNeighborNodeFunc(currentIndex, unitRadius);
                for (int i = 0; i < neighborIndexes.Count; i++)
                {
                    Vector2Int neighborIndex = neighborIndexes[i];

                    if (closeList.Contains(neighborIndex)) continue;

                    PathResultRecorder.AddSearchedCount();

                    float moveCost = currentIndex.GetNeighborMoveCost(neighborIndex);
                    float newG = nodeDict[currentIndex].g + moveCost;

                    if (!nodeDict.ContainsKey(neighborIndex) || newG + EPS < nodeDict[neighborIndex].g)
                    {
                        if (!nodeDict.ContainsKey(neighborIndex))
                        {
                            nodeDict[neighborIndex] = new PathNode
                            {
                                index = neighborIndex,
                                h = CaculateHeuristic(neighborIndex, goalNodeIndex),
                                g = float.PositiveInfinity
                            };
                        }
                        PathNode tempNode = nodeDict[neighborIndex];
                        tempNode.g = newG;
                        tempNode.parentIndex = currentIndex;
                        tempNode.beforeNodeIndex = currentIndex;
                        tempNode.isParentSet = true;
                        nodeDict[neighborIndex] = tempNode;

                        UpdateVertex(nodeDict, currentIndex, neighborIndex, goalNodeIndex);
                        openList.Enqueue(nodeDict[neighborIndex].index, nodeDict[neighborIndex].f);
                    }
                }
                Vector2IntListPool.ReleaseValue(neighborIndexes);
            }

            return null;
        }

        protected override List<Vector3> CaculateResult(Dictionary<Vector2Int, PathNode> nodeDict, Vector2Int current, Vector2Int start)
        {
            List<Vector2Int> path = Vector2IntListPool.GetValue();

            while (true)
            {
                path.Add(current);
                if (current == start) break;

                if (!nodeDict[current].isParentSet) break;
                current = nodeDict[current].parentIndex;
            }
            path.Reverse();

            List<Vector3> worldPath = Vector3ListPool.GetValue();
            for (int i = 0; i < path.Count; i++)
            {
                worldPath.Add(nodeList.GridToWorld(path[i]));
                if (nodeList.Nodes[path[i].x, path[i].y].GetNodeType() == NodeType.room)
                {
                    nodeList.NodeTypeController.SetNodeTypeInPathFinding(path[i], NodeType.trace);
                }
            }

            Vector2IntListPool.ReleaseValue(path);

            return worldPath;
        }

        private void UpdateVertex(Dictionary<Vector2Int, PathNode> nodeDict, Vector2Int current, Vector2Int neighbor, Vector2Int goalNodeIndex)
        {
            // current의 parent와 neighbor간에 line of Sight가 존재한다면 current의 parent에서 neighbor의 경로를 사용
            Vector2Int parentIndex = nodeDict[current].parentIndex;
            if (nodeDict[current].isParentSet && LineOfSight(parentIndex, neighbor))
            {
                float euclideanDistance = EuclideanDistance(parentIndex, neighbor);
                if (nodeDict[parentIndex].g + euclideanDistance <= nodeDict[neighbor].g + EPS)
                {
                    PathNode temp = nodeDict[neighbor];
                    temp.g = nodeDict[parentIndex].g + euclideanDistance;
                    temp.parentIndex = parentIndex;
                    temp.beforeNodeIndex = current;
                    temp.isParentSet = true;
                    temp.h = CaculateHeuristic(neighbor, goalNodeIndex);
                    nodeDict[neighbor] = temp;
                }
            }
            else
            {
                // If the length of the path from start to s and from s to 
                // neighbor is shorter than the shortest currently known distance
                // from start to neighbor, then update node with the new distance
                float euclideanDistance = EuclideanDistance(current, neighbor);
                if (nodeDict[current].g + euclideanDistance < nodeDict[neighbor].g + EPS)
                {
                    PathNode temp = nodeDict[neighbor];
                    temp.g = nodeDict[current].g + euclideanDistance;
                    temp.parentIndex = current;
                    temp.beforeNodeIndex = current;
                    temp.isParentSet = true;
                    temp.h = CaculateHeuristic(neighbor, goalNodeIndex);
                    nodeDict[neighbor] = temp;
                }
            }
        }

        private bool LineOfSight(Vector2Int node1, Vector2Int node2)
        {
            int x0 = node1.x;
            int y0 = node1.y;
            int x1 = node2.x;
            int y1 = node2.y;
            int dx = Mathf.Abs(x1 - x0);
            int dy = -Mathf.Abs(y1 - y0);

            int sX = -1;
            if (x0 < x1) sX = 1;
            int sY = -1;
            if (y0 < y1) sY = 1;

            int e = dx + dy;

            while (true)
            {
                // 이동 불가능한 Node이거나 Cluster가 비활성화인 경우 false, 두 Node사이에 시야가 없음
                var node = nodeList.Nodes[x0, y0];
                if (!node.IsWalkable)
                {
                    return false;
                }

                // 목적지 도착
                if (x0 == x1 && y0 == y1)
                {
                    return true;
                }

                // e는 현재 직선과 그리드에 대한 누적 오차값
                // e2는 비교를 위한 보정값, 두 방향(dx, dy)과의 상대적 위치를 판단하는데 사용
                int e2 = e * 2;

                // x방향 이동 조건
                if (e2 >= dy) // x방향으로 한 칸 이동해야 할 시점인지 판별
                {
                    if (x0 == x1)
                    {
                        return true;
                    }
                    e += dy;
                    x0 += sX;
                }

                // y방향 이동 조건
                if (e2 <= dx) // y방향으로 한 칸 이동해야 할 시점인지 판별
                {
                    if (y0 == y1)
                    {
                        return true;
                    }
                    e += dx;
                    y0 += sY;
                }
            }
        }

        protected override float CaculateHeuristic(Vector2Int from, Vector2Int to)
        {
            int dx = Mathf.Abs(to.x - from.x);
            int dy = Mathf.Abs(to.y - from.y);

            return dx + dy;
        }

        private float EuclideanDistance(Vector2Int node1, Vector2Int node2)
        {
            int dx = node2.x - node1.x;
            int dy = node2.y - node1.y;

            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
