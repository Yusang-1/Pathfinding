using Assets.Scripts.ControllUnit;
using UnityEngine;

public class UnitSpawnHolder
{
    // CreateMap에서 유닛 소환하고 위치를 지정하는 동안 code를 저장해두고
    // 위치 지정이 끝나면 spawn하는 기능

    [SerializeField] private UnitSpawner unitSpawner;

    private int currentReservedCode;

    public UnitSpawnHolder(UnitSpawner unitSpawner)
    {
        this.unitSpawner = unitSpawner;
    }

    public void ReserveSpawnUnitCode(int code)
    {
        currentReservedCode = code;
    }

    public void Spawn(Vector3 position)
    {
        unitSpawner.SpawnUnit(currentReservedCode, position);
    }
}
