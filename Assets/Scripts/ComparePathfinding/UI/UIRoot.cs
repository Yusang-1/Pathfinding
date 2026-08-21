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
    public event Action OnShowHPAThetaPathRequested;
    public event Action OnShowHAPStarSmoothingPathRequested;
    public event Action OnResetAllRequested;
    public event Action OnShowMoveUnitRequested;

    [SerializeField] private NodeTypeSelector nodeTypeSelector;
    [SerializeField] private UILoadMapMediator uiLoadMapMediator;
    [SerializeField] private UIFindAllPath uiFindAllPath;
    [SerializeField] private UIPathShower uiPathShower;
    [SerializeField] private UIResultController uiResultController;
    [SerializeField] private UIContainerScenes uiContainerScenes;
    
    private bool isInitialized;
    
    public void Initialize()
    {
        if (isInitialized) return;
        
        uiLoadMapMediator.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
        uiLoadMapMediator.OnOfficialMapListRequested += () => OnGetOfficialMapListRequested?.Invoke();
        uiLoadMapMediator.OnPersonalMapListRequested += () => OnGetPersonalMapListRequested?.Invoke();
        uiLoadMapMediator.OnOpenMapListRequested += uiFindAllPath.SetTempActiveFalse;
        uiLoadMapMediator.OnOpenMapListRequested += nodeTypeSelector.SetTempActiveFalse;
        uiLoadMapMediator.OnOpenMapListRequested += uiPathShower.SetTempActiveFalse;
        uiLoadMapMediator.OnOpenMapListRequested += uiResultController.SetTempActiveFalse;
        uiLoadMapMediator.OnLoadMapListClosedRequested += uiFindAllPath.ResetToBeforeActiveStatus;
        uiLoadMapMediator.OnLoadMapListClosedRequested += nodeTypeSelector.ResetToBeforeActiveStatus;
        uiLoadMapMediator.OnLoadMapListClosedRequested += uiPathShower.ResetToBeforeActiveStatus;
        uiLoadMapMediator.OnLoadMapListClosedRequested += uiResultController.ResetToBeforeActiveStatus;

        uiFindAllPath.OnFindAllPathEvent += () => OnFindAllPathRequested?.Invoke();

        uiPathShower.OnShowAStarPathRequested += () => OnShowAStarPathRequested?.Invoke();
        uiPathShower.OnShowHAPStarPathRequested += () => OnShowHAPStarPathRequested?.Invoke();
        uiPathShower.OnShowHPAThetaPathRequested += () => OnShowHPAThetaPathRequested?.Invoke();
        uiPathShower.OnShowHAPStarSmoothingPathRequested += () => OnShowHAPStarSmoothingPathRequested?.Invoke();
        uiPathShower.OnResetAllRequested += () => OnResetAllRequested?.Invoke();
        uiPathShower.OnResetAllRequested += () => ActiveResultController(false);
        uiPathShower.OnResetAllRequested += uiLoadMapMediator.ResetMediator;
        uiPathShower.OnShowMoveUnitRequested += () => OnShowMoveUnitRequested?.Invoke();

        nodeTypeSelector.OnGridToWorld += (grid) => (Vector2)OnGridToWorldRequested?.Invoke(grid);
        nodeTypeSelector.OnSetNodeType += (index, type) => OnSetNodeTypeRequested?.Invoke(index, type);

        uiFindAllPath.OnFindAllPathEvent += ActiveUIPathShower;
        
        isInitialized = true;
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

    public void SetAResult(PathResultRecorder.PathResult result)
    {
        uiResultController.SetAResult(result);
    }
    public void SetHPASmoothAStarResult(PathResultRecorder.PathResult result)
    {
        uiResultController.SetHPASmoothAStarResult(result);
    }
    public void SetHPAThetaResult(PathResultRecorder.PathResult result)
    {
        uiResultController.SetHPAThetaResult(result);
    }
    public void SetHPASmoothThetaResult(PathResultRecorder.PathResult result)
    {
        uiResultController.SetHPASmoothThetaResult(result);
    }

    public void ControllMenu() => uiContainerScenes.OnControllMenu();
}
