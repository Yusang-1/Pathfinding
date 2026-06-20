using System;
using UnityEngine;

public class UITileSelector : MonoBehaviour
{
    public event Action<NodeType> OnTileSelect;

    [SerializeField] private RectTransform obstacle;
    [SerializeField] private RectTransform room;
    [SerializeField] private RectTransform selectedHighlight;

    private float weighting;

    private void Start()
    {
        weighting = (selectedHighlight.sizeDelta.y - obstacle.sizeDelta.y) / 2;
        SetSelectedHighlight(obstacle);
    }

    /// <summary> button에 할당 </summary>
    public void OnSelectObstacle()
    {
        OnTileSelect?.Invoke(NodeType.obstacle);
        SetSelectedHighlight(obstacle);
    }

    /// <summary> button에 할당 </summary>
    public void OnSelectRoom()
    {
        OnTileSelect?.Invoke(NodeType.room);
        SetSelectedHighlight(room);
    }

    private void SetSelectedHighlight(Transform transform)
    {
        selectedHighlight.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + weighting);
    }

    public void SetActiveTrue() => gameObject.SetActive(true);
    public void SetActiveFalse() => gameObject.SetActive(false);
}
