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
    
    public void SetAResult(PathResult result)
    {
        aStarResultShower.SetResult(result);
    }
    public void SetHPAResult(PathResult result)
    {
        hPAStarResultShower.SetResult(result);
    }
    public void SetHPASmoothResult(PathResult result)
    {
        hPAStarSmoothingResultShower.SetResult(result);
    }
}
