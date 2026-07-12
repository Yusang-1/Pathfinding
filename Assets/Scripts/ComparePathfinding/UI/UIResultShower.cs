using UnityEngine;
using TMPro;

public class UIResultShower : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI searchedCount;
    [SerializeField] private TextMeshProUGUI pathLength;
    [SerializeField] private TextMeshProUGUI memoryUsed;
    
    public void SetResult(PathResultRecorder.PathResult result)
    {
        searchedCount.text = result.SearchedCount.ToString();
        pathLength.text = result.PathLength.ToString();
        memoryUsed.text = result.MemoryUsed.ToString();
    }
}
