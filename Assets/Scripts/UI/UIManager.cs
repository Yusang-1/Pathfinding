using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private NodeTypeSelector nodeTypeSelector;
    [SerializeField] private UIGenerateMap uiGenerateMap;
    [SerializeField] private UIFindAllPath uiFindAllPath;
    [SerializeField] private UIPathShower uiPathShower;
    [SerializeField] private UIResultController uIResultController;
    
    public void Initialize(NodeList nodeList, Action<int, int> generateMap, Action findAllPath, Action reseAll)
    {
        nodeTypeSelector.Initialize(nodeList);
        uiGenerateMap.Initialize(generateMap);
        uiFindAllPath.Initialize(findAllPath, ActiveResultController);
        uiPathShower.Initialize(reseAll, ActiveResultController);
        
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
    
    public void ActiveResultController(bool value)
    {
        uIResultController.gameObject.SetActive(value);
    }
}
