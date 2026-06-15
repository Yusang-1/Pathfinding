using UnityEngine;
using System;

[Serializable]
public struct MapData
{
    public string MapName;
    public int NodeSize;
    public int MapSize;
    public int ClusterSize;
    public Vector2Int[] ObstacleIndexes;
}
