using UnityEngine;

public class UIResultController : MonoBehaviour
{
    [SerializeField] private UIResultShower AStarResultShower;
    [SerializeField] private UIResultShower HPAStarResultShower;
    [SerializeField] private UIResultShower HPAStarSmoothingResultShower;
    
    public void ShowResult()
    {
        bool value = AStarResultShower.gameObject.activeSelf;
        
        AStarResultShower.gameObject.SetActive(!value);
        HPAStarResultShower.gameObject.SetActive(!value);
        HPAStarSmoothingResultShower.gameObject.SetActive(!value);
    }
}
