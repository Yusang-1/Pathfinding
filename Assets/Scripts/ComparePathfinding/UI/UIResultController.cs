using UnityEngine;

public class UIResultController : MonoBehaviour
{
    [SerializeField] private UIResultShower aStarResultShower;
    [SerializeField] private UIResultShower hPAStarResultShower;
    [SerializeField] private UIResultShower hPAStarSmoothingResultShower;
    
    public void ShowResult()
    {
        bool value = aStarResultShower.gameObject.activeSelf;
        
        aStarResultShower.gameObject.SetActive(!value);
        hPAStarResultShower.gameObject.SetActive(!value);
        hPAStarSmoothingResultShower.gameObject.SetActive(!value);
    }
    
    public void SetAResult(PathResultRecorder.PathResult result)
    {
        aStarResultShower.SetResult(result);
    }
    public void SetHPAResult(PathResultRecorder.PathResult result)
    {
        hPAStarResultShower.SetResult(result);
    }
    public void SetHPASmoothResult(PathResultRecorder.PathResult result)
    {
        hPAStarSmoothingResultShower.SetResult(result);
    }
    
    
    private bool beforeActiveStatus;
    public void SetTempActiveFalse()
    {
        beforeActiveStatus = gameObject.activeSelf;
        gameObject.SetActive(false);
    }

    public void ResetToBeforeActiveStatus()
    {
        gameObject.SetActive(beforeActiveStatus);
    }
}
