using UnityEngine;

public class UIResultController : MonoBehaviour
{
    [SerializeField] private UIResultShower aStarResultShower;
    [SerializeField] private UIResultShower hPASmoothAStarResultShower;
    [SerializeField] private UIResultShower hPAThetaResultShower;
    [SerializeField] private UIResultShower hPASmoothThetaResultShower;
    
    public void ShowResult()
    {
        bool value = aStarResultShower.gameObject.activeSelf;
        
        aStarResultShower.gameObject.SetActive(!value);
        hPASmoothAStarResultShower.gameObject.SetActive(!value);
        hPAThetaResultShower.gameObject.SetActive(!value);
        hPASmoothThetaResultShower.gameObject.SetActive(!value);
    }
    
    public void SetAResult(PathResultRecorder.PathResult result)
    {
        aStarResultShower.SetResult(result);
    }
    public void SetHPASmoothAStarResult(PathResultRecorder.PathResult result)
    {
        hPASmoothAStarResultShower.SetResult(result);
    }
    public void SetHPAThetaResult(PathResultRecorder.PathResult result)
    {
        hPAThetaResultShower.SetResult(result);
    }
    public void SetHPASmoothThetaResult(PathResultRecorder.PathResult result)
    {
        hPASmoothThetaResultShower.SetResult(result);
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
