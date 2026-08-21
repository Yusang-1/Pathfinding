using UnityEngine;

public abstract class AbstractSpawner : MonoBehaviour
{
    public abstract void SpawnUnit(UnitSize unitSize);
    public abstract void SetSpawnArea(Vector3 position);
}
