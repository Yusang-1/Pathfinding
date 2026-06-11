using UnityEngine;

public static class Vector2IntExtensions
{
    private const float VerticalCost = 1;
    private const float DiagonalCost = 1.4142f;
    public static float GetNeighborMoveCost(this Vector2Int from, Vector2Int to)
    {
        if (from == to) return 0;

        int dx = to.x - from.x;
        int dy = to.y - from.y;

        if (dx * dy == 0)
        {
            return VerticalCost;
        }
        else return DiagonalCost;
    }
}
