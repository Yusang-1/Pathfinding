using UnityEngine;
using System.Collections.Generic;

public interface IGetNeighborNodesActionProvider
{
    public List<Vector2Int> GetNeighborNodes(Vector2Int current, float unitRadius);
}

public class GetNeighborNodesProvider : IGetNeighborNodesActionProvider
{
    private readonly NodeList nodeList;
    private readonly HPAClusterList hPAClusterList;
    private readonly Vector2Int[] directions;

    public GetNeighborNodesProvider(NodeList nodeList, HPAClusterList hPAClusterList)
    {
        this.nodeList = nodeList;
        this.hPAClusterList = hPAClusterList;
        directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    }

    public List<Vector2Int> GetNeighborNodes(Vector2Int current, float unitRadius)
    {
        List<Vector2Int> neighbors = Vector2IntListPool.GetValue();

        for (int i = 0; i < directions.Length; i++)
        {
            int newX = current.x + directions[i].x;
            int newY = current.y + directions[i].y;

            if (newX < 0 || newY < 0 ||
                newX >= nodeList.Nodes.GetLength(0) || newY >= nodeList.Nodes.GetLength(0))
            {
                continue;
            }

            Vector2Int neighbor = new(newX, newY);
            // 워크어빌리티 맵으로 확인
            var s = nodeList.GridToWorld(neighbor);
            var c = hPAClusterList.GetClusterIndex((int)s.x, (int)s.y);
            if (nodeList.Nodes[newX, newY].IsWalkable && hPAClusterList.GetCluster(c).IsActive)
            {
                nodeList.NodeTypeController.SetNodeTypeInPathFinding(neighbor, NodeType.searched);
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
}

public class GetNeighborNodesInSameClusterProvider : IGetNeighborNodesActionProvider
{
    private readonly NodeList nodeList;
    private readonly HPAClusterList hPAClusterList;
    private readonly Vector2Int[] directions;

    public GetNeighborNodesInSameClusterProvider(NodeList nodeList, HPAClusterList hPAClusterList)
    {
        this.nodeList = nodeList;
        this.hPAClusterList = hPAClusterList;
        directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    }

    public List<Vector2Int> GetNeighborNodes(Vector2Int current, float unitRadius)
    {
        List<Vector2Int> neighbors = Vector2IntListPool.GetValue();

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
            if (unitRadius == 0)
            {
                if (nodeList.Nodes[newX, newY].IsWalkable)
                {
                    nodeList.NodeTypeController.SetNodeTypeInPathFinding(neighbor, NodeType.searched);
                    neighbors.Add(neighbor);
                }
            }
            else
            {
                if (CanUnitFitAtNode(neighbor, unitRadius))
                {
                    neighbors.Add(neighbor);
                }
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
}

public class GetNeighborNodesWithClusterListProvider : IGetNeighborNodesActionProvider
{
    private readonly NodeList nodeList;
    private readonly HPAClusterList hPAClusterList;
    private readonly Vector2Int[] directions;
    private List<Vector2Int> clusterListToFind;

    public GetNeighborNodesWithClusterListProvider(NodeList nodeList, HPAClusterList hPAClusterList)
    {
        this.nodeList = nodeList;
        this.hPAClusterList = hPAClusterList;
        directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    }

    public void SetClusterList(List<Vector2Int> clusters)
    {
        clusterListToFind = clusters ?? new List<Vector2Int>();
    }

    public List<Vector2Int> GetNeighborNodes(Vector2Int current, float unitRadius)
    {
        List<Vector2Int> neighbors = Vector2IntListPool.GetValue();

        for (int i = 0; i < directions.Length; i++)
        {
            int newX = current.x + directions[i].x;
            int newY = current.y + directions[i].y;

            Vector2Int neighbor = new(newX, newY);

            if (newX < 0 || newY < 0 || newX >= nodeList.Nodes.GetLength(0) || newY >= nodeList.Nodes.GetLength(0))
            {
                continue;
            }

            var nodeWorldPosition = nodeList.GridToWorld(current);
            var clusterIndex = hPAClusterList.GetClusterIndex((int)nodeWorldPosition.x, (int)nodeWorldPosition.y);
            
            bool walkableCheck = unitRadius == 0 ? nodeList.Nodes[newX, newY].IsWalkable : CanUnitFitAtNode(neighbor, unitRadius);
            
            // 워크어빌리티 맵으로 확인
            if (walkableCheck && hPAClusterList.GetCluster(clusterIndex).IsActive)
            {
                bool isNeighborInClusters = false;
                foreach (var cluster in clusterListToFind)
                {
                    isNeighborInClusters = isNeighborInClusters || hPAClusterList.IsNodeInCluster(cluster, neighbor);

                    if (isNeighborInClusters) break;
                }
                if (!isNeighborInClusters) continue;

                nodeList.NodeTypeController.SetNodeTypeInPathFinding(neighbor, NodeType.searched);
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
}


