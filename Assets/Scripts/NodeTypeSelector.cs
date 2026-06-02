using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NodeTypeSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{   
    private Vector2Int currentIndex;
    private NodeList nodeList;
    
    [SerializeField] private float uiSpacing;
    
    [SerializeField] private Image typeSelectorImage;
    [SerializeField] private Image[] childrenImage; 
    [SerializeField] private float translucentValue;
    
    public void Initialize(NodeList nodeList)
    {
        this.nodeList = nodeList;
        nodeList.OnSelected += OpenSelector;
        nodeList.OnDeselected += CloseSelector;
    }
    
    private void OpenSelector(Vector2Int index)
    {
        gameObject.SetActive(true);
        currentIndex = index;
        SetTypeSelectorPosition(index);
    }
    private void CloseSelector(Vector2Int index) => gameObject.SetActive(false);

    public void ButtonUnit()
    {
        nodeList.SetNodeType(currentIndex, NodeType.unit);
        gameObject.SetActive(false);
    }

    public void ButtonDestination()
    {
        nodeList.SetNodeType(currentIndex, NodeType.destination);        
        gameObject.SetActive(false);
    }

    public void ButtonObstacle()
    {
        nodeList.SetNodeType(currentIndex, NodeType.obstacle);
        gameObject.SetActive(false);
    }

    public void ButtonRoom()
    {
        nodeList.SetNodeType(currentIndex, NodeType.room);
        gameObject.SetActive(false);
    }
    
    private void SetTypeSelectorPosition(Vector2Int nodeIndex)
    {
        Vector3 uiPosition = Camera.main.WorldToScreenPoint(nodeList.GridToWorld(nodeIndex));
        uiPosition.x += uiSpacing;
        uiPosition.y -= uiSpacing;
        
        transform.position = uiPosition;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Color tempColor = typeSelectorImage.color;
        tempColor.a = translucentValue;
        typeSelectorImage.color = tempColor;
        
        for(int i = 0; i < childrenImage.Length; i++)
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
        
        for(int i = 0; i < childrenImage.Length; i++)
        {
            tempColor = childrenImage[i].color;
            tempColor.a = 1;
            childrenImage[i].color = tempColor;
        }
    }
}
