using UnityEngine;
using System;

public class UILoadMapList : MonoBehaviour
{
    public event Action OnLoadMapFinished;
    public event Action OnLoadMapListClosed;
    public event Action<MapData> OnLoadMapRequested;

    [SerializeField] private Transform officialMapContainer;
    [SerializeField] private Transform personalMapContainer;
    [SerializeField] private UIMapContainer mapContainer;
    [SerializeField] private UIMapInfo mapInfo;

    private readonly UIMapContainer[] officialContainerPool;
    private readonly UIMapContainer[] personalContainerPool;

    public void ShowMapList(MapData[] officialMaps, MapData[] personalMaps)
    {
        ShowMap(officialMaps, officialContainerPool, officialMapContainer);

        ShowMap(personalMaps, personalContainerPool, personalMapContainer);

        gameObject.SetActive(true);
    }

    private void ShowMap(MapData[] maps, UIMapContainer[] pool, Transform transform)
    {
        if (maps == null || maps.Length == 0) return;

        if (pool == null)
        {
            pool = new UIMapContainer[maps.Length];
            SetMapContainerPosition(0, pool, transform);
        }
        else if (pool.Length < maps.Length)
        {
            int lastSortedIndex = pool.Length - 1;
            var newArray = new UIMapContainer[maps.Length];
            Array.Copy(pool, newArray, pool.Length);
            pool = newArray;
            SetMapContainerPosition(lastSortedIndex, pool, transform);
        }

        for (int i = 0; i < maps.Length; i++)
        {
            pool[i].Initialize(maps[i], OnSelect);
        }
    }

    private void SetMapContainerPosition(int startIndex, UIMapContainer[] containers, Transform transform)
    {
        float height = mapContainer.GetComponent<RectTransform>().sizeDelta.y;

        Vector2 position = Vector2.zero;
        for (int i = startIndex; i < containers.Length; i++)
        {
            position.y = -height * i;
            containers[i] = Instantiate(mapContainer, transform);
            containers[i].GetComponent<RectTransform>().anchoredPosition = position;
        }
    }

    private MapData currentSelected;
    private void OnSelect(MapData mapData)
    {
        currentSelected = mapData;
        mapInfo.SetInfo(currentSelected);
    }

    /// <summary> button에 할당 </summary>
    public void OnCloseLoadMapList()
    {
        gameObject.SetActive(false);
        OnLoadMapListClosed?.Invoke();
    }

    /// <summary> button에 할당 </summary>
    public void OnLoadSelectedMap()
    {
        OnLoadMapRequested?.Invoke(currentSelected);
        OnCloseLoadMapList();
        OnLoadMapFinished?.Invoke();
    }
}

