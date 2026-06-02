using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private NodeTypeSelector nodeTypeSelector;
    [SerializeField] private UIGenerateMap uiGenerateMap;
    [SerializeField] private UIFindAllPath uiFindAllPath;
    [SerializeField] private UIPathShower uiPathShower;
    
    public void Initialize(NodeList nodeList, Action generateMap, Action findAllPath, Action reseAll)
    {
        nodeTypeSelector.Initialize(nodeList);
        uiGenerateMap.Initialize(generateMap);
        uiFindAllPath.Initialize(findAllPath);
        uiPathShower.Initialize(reseAll);
        
        uiFindAllPath.OnFindAllPathEvent += ActiveUIPathShower;
    }
    
    private void ActiveUIPathShower()
    {
        uiPathShower.gameObject.SetActive(true);
    }
    public void ActiveFindUI(bool value)
    {
        uiFindAllPath.gameObject.SetActive(value);
    }
}
