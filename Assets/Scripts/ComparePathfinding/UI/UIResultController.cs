using UnityEngine;

public class UIResultController : MonoBehaviour
{
    [SerializeField] private GameObject resultsContainer;
    [SerializeField] private UIResultShower aStarResultShower;
    [SerializeField] private UIResultShower hPASmoothAStarResultShower;
    [SerializeField] private UIResultShower hPAThetaResultShower;
    [SerializeField] private UIResultShower hPASmoothThetaResultShower;
    
    public void ShowResult()
    {
        bool value = resultsContainer.activeSelf;
        
        resultsContainer.SetActive(!value);
        
        if(value)
        {
            if(!aStarResultShower.gameObject.activeSelf)
            {
                aStarResultShower.gameObject.SetActive(true);
            }
            if(!hPASmoothAStarResultShower.gameObject.activeSelf)
            {
                hPASmoothAStarResultShower.gameObject.SetActive(true);
            }
            if(!hPAThetaResultShower.gameObject.activeSelf)
            {
                hPAThetaResultShower.gameObject.SetActive(true);
            }
            if(!hPASmoothThetaResultShower.gameObject.activeSelf)
            {
                hPASmoothThetaResultShower.gameObject.SetActive(true);
            }
        }
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
