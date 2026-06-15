using UnityEngine;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] private PathLine linePrefab;

    [SerializeField] private float width;
    
    private readonly List<PathLine> lines = new();
    private readonly ObjectPool<PathLine> linePool = new();

    public void DrawLine(List<Vector2Int> path)
    {
        Vector3 pos1, pos2;
        for (int i = 0; i < path.Count - 1; i++)
        {
            pos1 = new Vector3(path[i].x, path[i].y, 0);
            pos2 = new Vector3(path[i + 1].x, path[i + 1].y, 0);
            Draw(pos1, pos2);
        }
    }
    public void DrawLine(List<Vector3> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Draw(path[i], path[i + 1]);
        }
    }

    private void Draw(Vector3 start, Vector3 end)
    {
        if (!linePool.TryGetObject(out PathLine line))
        {
            line = Instantiate(linePrefab);

            line.OnPoolObjectFirstCreated += linePool.PoolObjectFirstCreated;
            line.OnPoolObjectUnused += linePool.PoolObjectUnused;

            line.Initialize();
        }

        Vector3 scale = new(width, Vector3.Distance(start, end), 1);
        Vector3 direction = (end - start).normalized;
        line.SetPosition(scale, start, end, direction);
        
        lines.Add(line);
    }

    public void ResetLineDrawer()
    {
        for(int i = 0; i < lines.Count; i++)
        {
            lines[i].ResetLine();
        }
        
        lines.Clear();
    }        
}
