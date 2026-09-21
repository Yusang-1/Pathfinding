using Assets.Scripts.ControllUnit;
using UnityEngine;

public class UnitSpawnHolder
{    
    private readonly UnitPreviewShower previewShower;    

    public UnitSpawnHolder(UnitSpawner unitSpawner)
    {        
        previewShower = new UnitPreviewShower(unitSpawner);
    }
    
    public void MovePreviewUnit(Vector3 position)
    {
        previewShower.MovePreviewUnit(position);
    }

    public void ReserveSpawnUnitCode(int code)
    {
        previewShower.ShowUnitPreview(code, Vector3.zero);
    }

    public void Spawn()
    {
        previewShower.PreviewToUnit();
    }
    
    public void CancelSpawn()
    {
        previewShower.CancelSpawn();
    }
    
    public void HidePreviewUnit(bool value)
    {
        previewShower.HidePreviewUnit(value);
    }
}
