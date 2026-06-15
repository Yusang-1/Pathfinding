using UnityEngine;
using System;

public class UIRoot : MonoBehaviour
{
    public event Action OnFindAllPathRequested;
    
    public event Action<Vector2Int, NodeType> OnSetNodeTypeRequested;
    public event Func<Vector2Int, Vector2> OnGridToWorldRequested;
    
    // UILoadMapMediator event
    public event Action<MapData> OnLoadMapRequested;
    public event Func<MapData[]> OnGetOfficialMapListRequested;
    public event Func<MapData[]> OnGetPersonalMapListRequested;
    
    // UIPathShower event
    public event Action OnShowAStarPathRequested;
    public event Action OnShowHAPStarPathRequested;
    public event Action OnShowHAPStarSmoothingPathRequested;
    public event Action OnResetAllRequested;
    public event Action OnShowMoveUnitRequested;

    [SerializeField] private NodeTypeSelector nodeTypeSelector;
    [SerializeField] private UILoadMapMediator uiLoadMapMediator;
    [SerializeField] private UIFindAllPath uiFindAllPath;
    [SerializeField] private UIPathShower uiPathShower;
    [SerializeField] private UIResultController uiResultController;

    public void Initialize()
    {
        uiLoadMapMediator.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
        uiLoadMapMediator.OnOfficialMapListRequested += () => OnGetOfficialMapListRequested?.Invoke();
        uiLoadMapMediator.OnPersonalMapListRequested += () => OnGetPersonalMapListRequested?.Invoke();

        uiFindAllPath.OnFindAllPathEvent += () => OnFindAllPathRequested?.Invoke();
        
        uiPathShower.OnShowAStarPathRequested += () => OnShowAStarPathRequested?.Invoke();
        uiPathShower.OnShowHAPStarPathRequested += () => OnShowHAPStarPathRequested?.Invoke();
        uiPathShower.OnShowHAPStarSmoothingPathRequested += () => OnShowHAPStarSmoothingPathRequested?.Invoke();        
        uiPathShower.OnResetAllRequested += () => OnResetAllRequested?.Invoke();
        uiPathShower.OnResetAllRequested += () => ActiveResultController(false);
        uiPathShower.OnResetAllRequested += uiLoadMapMediator.ResetMediator;
        uiPathShower.OnShowMoveUnitRequested += () => OnShowMoveUnitRequested?.Invoke();
        
        nodeTypeSelector.OnGridToWorld += (grid) => (Vector2)OnGridToWorldRequested?.Invoke(grid);
        nodeTypeSelector.OnSetNodeType += (index, type) => OnSetNodeTypeRequested?.Invoke(index, type);

        uiFindAllPath.OnFindAllPathEvent += ActiveUIPathShower;
        
        
    }

    private void ActiveUIPathShower()
    {
        uiPathShower.gameObject.SetActive(true);
    }
    public void ActiveFindButton(bool value)
    {
        uiFindAllPath.gameObject.SetActive(value);
    }

    public void ActiveResultController(bool value)
    {
        uiResultController.gameObject.SetActive(value);
    }

    public void ActiveNodeTypeSelector(ISelectable selectable, bool value)
    {
        Vector2Int index = (selectable as Node).Index;
        nodeTypeSelector.ActiveSelector(index, value);
    }

    public void SetAResult(PathResult result)
    {
        uiResultController.SetAResult(result);
    }
    public void SetHPAResult(PathResult result)
    {
        uiResultController.SetHPAResult(result);
    }
    public void SetHPASmoothResult(PathResult result)
    {
        uiResultController.SetHPASmoothResult(result);
    }
}
