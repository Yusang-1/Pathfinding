using UnityEngine;

public class MapGenerator
{
    public void GenerateMap(int mapSize, Node nodePrefab , NodeList nodeList)
    {
        Node node;
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                node = Node.Instantiate(nodePrefab, new Vector3(i, j, 0), Quaternion.identity);
                nodeList.SetNode(i, j, node);
            }
        }        
    }
}
