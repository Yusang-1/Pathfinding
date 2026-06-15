using UnityEngine;
using System;

public class UIRoot : MonoBehaviour
{
    public event Action<int, int> OnGenerateMapRequested;
    public event Action OnFindAllPathRequested;
    public event Action OnResetAllRequested;
    public event Action<Vector2Int, NodeType> OnSetNodeTypeRequested;
    public event Func<Vector2Int, Vector2> OnGridToWorldRequested;
    public event Action<MapData> OnLoadMapRequested;
    public event Func<MapData[]> OnGetOfficialMapListRequested;
    public event Func<MapData[]> OnGetPersonalMapListRequested;

    [SerializeField] private NodeTypeSelector nodeTypeSelector;
    [SerializeField] private UIGenerateMapMediator uIGenerateMapMediator;
    [SerializeField] private UIFindAllPath uiFindAllPath;
    [SerializeField] private UIPathShower uiPathShower;
    [SerializeField] private UIResultController uIResultController;

    public void Initialize()
    {
        uIGenerateMapMediator.OnGenerateMapRequested += (mapSize, clusterSize) => OnGenerateMapRequested?.Invoke(mapSize, clusterSize);
        uIGenerateMapMediator.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
        uIGenerateMapMediator.OnOfficialMapListRequested += () => OnGetOfficialMapListRequested?.Invoke();
        uIGenerateMapMediator.OnPersonalMapListRequested += () => OnGetPersonalMapListRequested?.Invoke();

        uiFindAllPath.OnFindAllPathEvent += () => OnFindAllPathRequested?.Invoke();

        uiPathShower.OnResetAll += () => OnResetAllRequested?.Invoke();
        uiPathShower.OnResetAll += () => ActiveResultController(false);

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
        uIResultController.gameObject.SetActive(value);
    }

    public void ActiveNodeTypeSelector(ISelectable selectable, bool value)
    {
        Vector2Int index = (selectable as Node).Index;
        nodeTypeSelector.ActiveSelector(index, value);
    }

    public void SetAResult(PathResult result)
    {
        uIResultController.SetAResult(result);
    }
    public void SetHPAResult(PathResult result)
    {
        uIResultController.SetHPAResult(result);
    }
    public void SetHPASmoothResult(PathResult result)
    {
        uIResultController.SetHPASmoothResult(result);
    }
}
