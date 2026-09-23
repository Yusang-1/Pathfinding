using System;
using Assets.Scripts.ControllUnit;
using Assets.Scripts.CreateMap;

public class PathManagerBootStrapper
{
    private Action<bool> pathfindAvailableHandler;
    private Action<ISelectable, bool> selectedHandler;
    private Action<ISelectable, bool> deselectedHandler;
    private readonly Action<int> setMapData;

    private readonly NodeList nodeList;
    private readonly MapdataJsonConverter mapdataJsonConverter;
    private readonly UIRoot uiRoot;
    private readonly InputManager inputManager;
    private readonly PathfinderComparePathfinding pathfinder;

    private bool isEventBound;

    public PathManagerBootStrapper(NodeList nodeList, UIRoot uiRoot, InputManager inputManager, PathfinderComparePathfinding pathfinder,
        Action<int> setMapData, MapRuntimeContext mapRuntimeContext)
    {
        this.nodeList = nodeList;
        this.uiRoot = uiRoot;
        this.inputManager = inputManager;
        this.pathfinder = pathfinder;
        this.setMapData = setMapData;
        mapdataJsonConverter = new(mapRuntimeContext.LoadedMapData);
        
        inputManager.Initialize(nodeList);
    }

    public void BindEvents()
    {
        if (isEventBound) return;

        selectedHandler = (node, value) => uiRoot.ActiveNodeTypeSelector(node, value);
        deselectedHandler = (node, value) => uiRoot.ActiveNodeTypeSelector(node, value);
        pathfindAvailableHandler = (value) => uiRoot.ActiveFindButton(value);

        AddUIRootEvent();
        AddNodeListEvent();
        AddPathfinderEvent();
        AddInputManagerEvent();

        isEventBound = true;
    }

    public void UnbindEvents()
    {
        if (!isEventBound) return;

        RemoveUIRootEvent();
        RemoveNodeListEvent();
        RemovePathfinderEvent();
        RemoveInputManagerEvent();

        isEventBound = false;
    }

    private void AddUIRootEvent()
    {
        uiRoot.OnFindAllPathRequested += pathfinder.FindAllPath;
        uiRoot.OnSetNodeTypeRequested += nodeList.NodeTypeController.NodeTypeDrawer.SetNodeType;
        uiRoot.OnGridToWorldRequested += nodeList.GridToWorld;
        uiRoot.OnLoadMapRequested += setMapData;
        uiRoot.OnGetPersonalMapListRequested += mapdataJsonConverter.GetPersonalSavedMaps;
        uiRoot.OnGetOfficialMapListRequested += mapdataJsonConverter.GetOfficialSavedMaps;

        uiRoot.OnShowAStarPathRequested += pathfinder.ShowAStarResult;
        uiRoot.OnShowHAPStarPathRequested += pathfinder.ShowHPASmoothingAStarResult;
        uiRoot.OnShowHPAThetaPathRequested += pathfinder.ShowHPAThetaResult;
        uiRoot.OnShowHAPStarSmoothingPathRequested += pathfinder.ShowHPASmoothingThetaResult;
        uiRoot.OnResetAllRequested += pathfinder.ResetAll;
        uiRoot.OnShowMoveUnitRequested += pathfinder.MoveUnitLazyRefine;
    }

    private void AddNodeListEvent()
    {
        nodeList.NodeTypeController.NodeTypeDrawer.OnPathfindAvailable += pathfindAvailableHandler;
        nodeList.OnSelected += selectedHandler;
        nodeList.OnDeselected += deselectedHandler;
    }

    private void AddPathfinderEvent()
    {
        pathfinder.OnPathFound += uiRoot.ActiveResultController;
        pathfinder.OnAFound += uiRoot.SetAResult;
        pathfinder.OnHPASmoothAStarFound += uiRoot.SetHPASmoothAStarResult;
        pathfinder.OnHPAThetaFound += uiRoot.SetHPAThetaResult;
        pathfinder.OnHPASmoothThetaFound += uiRoot.SetHPASmoothThetaResult;
    }

    private void AddInputManagerEvent()
    {
        inputManager.OnControllMenu += uiRoot.ControllMenu;
    }

    private void RemoveUIRootEvent()
    {
        uiRoot.OnFindAllPathRequested -= pathfinder.FindAllPath;
        uiRoot.OnSetNodeTypeRequested -= nodeList.NodeTypeController.NodeTypeDrawer.SetNodeType;
        uiRoot.OnGridToWorldRequested -= nodeList.GridToWorld;
        uiRoot.OnLoadMapRequested -= setMapData;
        uiRoot.OnGetPersonalMapListRequested -= mapdataJsonConverter.GetPersonalSavedMaps;
        uiRoot.OnGetOfficialMapListRequested -= mapdataJsonConverter.GetOfficialSavedMaps;

        uiRoot.OnShowAStarPathRequested -= pathfinder.ShowAStarResult;
        uiRoot.OnShowHAPStarPathRequested -= pathfinder.ShowHPASmoothingAStarResult;
        uiRoot.OnShowHPAThetaPathRequested -= pathfinder.ShowHPAThetaResult;
        uiRoot.OnShowHAPStarSmoothingPathRequested -= pathfinder.ShowHPASmoothingThetaResult;
        uiRoot.OnResetAllRequested -= pathfinder.ResetAll;
        uiRoot.OnShowMoveUnitRequested -= pathfinder.MoveUnitLazyRefine;
    }

    private void RemoveNodeListEvent()
    {
        nodeList.NodeTypeController.NodeTypeDrawer.OnPathfindAvailable -= pathfindAvailableHandler;
        nodeList.OnSelected -= selectedHandler;
        nodeList.OnDeselected -= deselectedHandler;
    }

    private void RemovePathfinderEvent()
    {
        pathfinder.OnPathFound -= uiRoot.ActiveResultController;
        pathfinder.OnAFound -= uiRoot.SetAResult;
        pathfinder.OnHPASmoothAStarFound -= uiRoot.SetHPASmoothAStarResult;
        pathfinder.OnHPAThetaFound -= uiRoot.SetHPAThetaResult;
        pathfinder.OnHPASmoothThetaFound -= uiRoot.SetHPASmoothThetaResult;
    }

    private void RemoveInputManagerEvent()
    {
        inputManager.OnControllMenu -= uiRoot.ControllMenu;
    }
}
