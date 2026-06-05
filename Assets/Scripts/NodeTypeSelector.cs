using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class NodeTypeSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<Vector2Int, NodeType> OnSetNodeType;
    public event Func<Vector2Int, Vector2> OnGridToWorld;

    private Vector2Int currentIndex;

    [SerializeField] private float uiSpacing;

    [SerializeField] private Image typeSelectorImage;
    [SerializeField] private Image[] childrenImage;
    [SerializeField] private float translucentValue;

    public void ActiveSelector(Vector2Int index, bool value)
    {
        gameObject.SetActive(value);
        
        if (value)
        {
            currentIndex = index;
            SetTypeSelectorPosition(index);
        }
    }

    public void ButtonUnit()
    {
        OnSetNodeType?.Invoke(currentIndex, NodeType.unit);
        gameObject.SetActive(false);
    }

    public void ButtonDestination()
    {
        OnSetNodeType?.Invoke(currentIndex, NodeType.destination);
        gameObject.SetActive(false);
    }

    public void ButtonObstacle()
    {
        OnSetNodeType?.Invoke(currentIndex, NodeType.obstacle);
        gameObject.SetActive(false);
    }

    public void ButtonRoom()
    {
        OnSetNodeType?.Invoke(currentIndex, NodeType.room);
        gameObject.SetActive(false);
    }

    private void SetTypeSelectorPosition(Vector2Int nodeIndex)
    {
        Vector3 uiPosition = Camera.main.WorldToScreenPoint((Vector2)OnGridToWorld?.Invoke(nodeIndex));
        uiPosition.x += uiSpacing;
        uiPosition.y -= uiSpacing;

        transform.position = uiPosition;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Color tempColor = typeSelectorImage.color;
        tempColor.a = translucentValue;
        typeSelectorImage.color = tempColor;

        for (int i = 0; i < childrenImage.Length; i++)
        {
            tempColor = childrenImage[i].color;
            tempColor.a = translucentValue;
            childrenImage[i].color = tempColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Color tempColor = typeSelectorImage.color;
        tempColor.a = 1;
        typeSelectorImage.color = tempColor;

        for (int i = 0; i < childrenImage.Length; i++)
        {
            tempColor = childrenImage[i].color;
            tempColor.a = 1;
            childrenImage[i].color = tempColor;
        }
    }
}
