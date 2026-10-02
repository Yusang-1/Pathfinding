using UnityEngine;
using TMPro;

public class UIResultShower : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI searchedCount;
    [SerializeField] private TextMeshProUGUI pathLength;
    [SerializeField] private TextMeshProUGUI memoryUsed;

    public void SetResult(PathResultRecorder.PathResult result)
    {
        char[] count = CachedTextNumber.GetCachedText(result.SearchedCount, out int cLength);
        searchedCount.SetText(count, 0, cLength);
        
        char[] path = CachedTextNumber.GetCachedText(result.PathLength, out int pLength);
        pathLength.SetText(path, 0, pLength);
        
        char[] memory = CachedTextNumber.GetCachedText(result.MemoryUsed, out int mLength);
        memoryUsed.SetText(memory, 0, mLength);
    }
}
