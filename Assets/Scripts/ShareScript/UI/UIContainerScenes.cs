using UnityEngine;

public class UIContainerScenes : MonoBehaviour
{
    /// <summary> button에 할당 </summary>
    public void OnLoadCreateMapScene()
    {
        SceneChanger.ChangeScene("CreateMap");
    }
    /// <summary> button에 할당 </summary>
    public void OnLoadComparePathfindingScene()
    {
        SceneChanger.ChangeScene("ComparePathfinding");
    }
    /// <summary> button에 할당 </summary>
    public void OnLoadControllUnitScene()
    {
        SceneChanger.ChangeScene("ControllUnit");
    }
    /// <summary> button에 할당 </summary>
    public void OnLoadTitleScene()
    {
        SceneChanger.ChangeScene("Title");
    }
    
    /// <summary> event에 할당 </summary>
    public void OnControllMenu()
    {
        if (gameObject.activeSelf)
        {
            OnCloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }
    
    /// <summary> button에 할당 </summary>
    public void OnCloseMenu()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
    
    private void OpenMenu()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0;
    }
}
