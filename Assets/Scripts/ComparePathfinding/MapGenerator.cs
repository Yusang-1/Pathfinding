using UnityEngine;

public class MapGenerator
{
    private readonly Node nodePrefab;
    private readonly NodeList nodeList;
    private readonly ObjectPool<Node> nodePool = new();
    
    public MapGenerator(Node prefab, NodeList list)
    {
        nodePrefab = prefab;
        nodeList = list;
    }

    public void GenerateMap(int mapSize)
    {
        // nodeList.CreateNodeArray(mapSize);

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (!nodePool.TryGetObject(out Node node))
                {
                    // nodePool에서 가져올게 없다면
                    node = Node.Instantiate(nodePrefab, new Vector3(i, j, 0), Quaternion.identity);
                    node.OnPoolObjectFirstCreated += nodePool.AddToPool;
                    node.OnPoolObjectUnused += nodePool.UsedToUnused;
                }
                
                node.transform.position = new Vector3(i, j, 0);
                nodeList.SetNode(i, j, node);
            }
        }
    }

    public void GenerateMap(MapData mapData)
    {
        GenerateMap(mapData.MapSize);

        foreach (var index in mapData.ObstacleIndexes)
        {
            nodeList.SetNodeType(index, NodeType.obstacle);
        }
    }
}
