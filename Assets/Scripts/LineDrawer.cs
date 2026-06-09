using UnityEngine;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject lineHeadPrefab;
    [SerializeField] private float width;

    private readonly List<GameObject> lines = new();
    private readonly List<GameObject> lineHeads = new();
    private int current;

    public void Initialize()
    {
        GameObject line;
        GameObject lineHead;
        for (int i = 0; i < 10; i++)
        {
            line = Instantiate(linePrefab);
            line.SetActive(false);
            lines.Add(line);

            lineHead = Instantiate(lineHeadPrefab);
            lineHead.SetActive(false);
            lineHeads.Add(lineHead);
        }
    }

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
        if (lines.Count <= current)
        {
            lines.Add(Instantiate(linePrefab));
        }

        GameObject line = lines[current];
        line.SetActive(true);

        line.transform.localScale = new Vector3(width, Vector3.Distance(start, end), 1);
        Vector3 direction = (end - start).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        line.transform.SetPositionAndRotation((start + end) / 2, Quaternion.Euler(0, 0, angle));

        GameObject head = lineHeads[current++];
        head.SetActive(true);
        head.transform.SetPositionAndRotation(end, Quaternion.Euler(0, 0, angle));
    }

    public void ResetLineDrawer()
    {
        current = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].activeSelf)
            {
                lines[i].SetActive(false);
            }
            if (i < lineHeads.Count && lineHeads[i].activeSelf)
            {
                lineHeads[i].SetActive(false);
            }
        }
    }
}
