using Assets.Scripts.ControllUnit;
using UnityEngine;

public class UnitPreviewShower
{
    private readonly UnitSpawner unitSpawner;
    private Unit currentPreviewUnit;
            
    public UnitPreviewShower(UnitSpawner unitSpawner)
    {
        this.unitSpawner = unitSpawner;        
    }
    
    public void MovePreviewUnit(Vector3 position)
    {
        if(currentPreviewUnit == null) return;
        
        currentPreviewUnit.SimpleMove(position);
    }
    
    public void ShowUnitPreview(int unitCode, Vector3 position)
    {
        currentPreviewUnit = unitSpawner.SpawnUnitPreview(unitCode, position);
        currentPreviewUnit.SetTranslucent();
    }        
    
    public void PreviewToUnit()
    {
        unitSpawner.UnitPreviewToUnit(currentPreviewUnit);
        currentPreviewUnit.SetOpaque();
    }
    
    public void CancelSpawn()
    {
        currentPreviewUnit.SetOpaque();
        currentPreviewUnit.UnitDespawned();
        currentPreviewUnit = null;
    }
    
    public void HidePreviewUnit(bool value)
    {
        currentPreviewUnit.gameObject.SetActive(value);
    }        
}
