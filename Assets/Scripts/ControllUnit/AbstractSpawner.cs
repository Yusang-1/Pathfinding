using UnityEngine;

public abstract class AbstractSpawner : MonoBehaviour
{
    public abstract void SpawnUnit(int unitCode);
    public abstract void SetSpawnArea(Vector3 position);
}
