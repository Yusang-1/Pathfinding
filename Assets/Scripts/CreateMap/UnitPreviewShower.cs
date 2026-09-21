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
        
        currentPreviewUnit.transform.position = position;
    }
    
    public void ShowUnitPreview(int unitCode, Vector3 position)
    {
        currentPreviewUnit = unitSpawner.SpawnUnitPreview(unitCode, position);
        // SetTranslucent();
    }        
    
    public void PreviewToUnit()
    {
        unitSpawner.UnitPreviewToUnit(currentPreviewUnit);
        // SetOpaque();
    }
    
    public void CancelSpawn()
    {
        // SetOpaque();
        currentPreviewUnit.UnitDespawned();
        currentPreviewUnit = null;
    }
    
    public void HidePreviewUnit(bool value)
    {
        currentPreviewUnit.gameObject.SetActive(value);
    }        
}
