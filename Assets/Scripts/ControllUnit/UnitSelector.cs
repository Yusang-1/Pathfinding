using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public class UnitSelector
    {
        private readonly SpatialHash spatialHash;
        private readonly SelectableController selectableController;

        public UnitSelector(SpatialHash spatialHash, SelectableController selectableController)
        {
            this.spatialHash = spatialHash;
            this.selectableController = selectableController;
        }

        public void GetActions(Action<ActionMaps> changeActionMapSelected, Action changeActionMapDefault)
        {
            selectableController.GetActions(changeActionMapSelected, changeActionMapDefault);
        }

        public void SelectFocused()
        {
            selectableController.Selected();
        }

        public void ShiftSelectedFocused()
        {
            selectableController.ShiftSelected();
        }

        public void ShiftSelectedFocusedList()
        {
            selectableController.ShiftSelectedList();
        }

        public void CheckPointFocused(Vector3 checkPosition)
        {
            Vector2Int hash = spatialHash.GetHashKey(checkPosition);
            var unitsInCell = spatialHash.GetUnitsInCell(hash);

            if (unitsInCell == null || unitsInCell.Count == 0)
            {
                selectableController.UnitFocusedPoint(null);
                return;
            }

            Unit focusedUnit = null;
            float closestDistanceSq = float.MaxValue;

            foreach (Unit unit in unitsInCell)
            {
                var unitPosition = unit.transform.position;
                unitPosition.z = 0;

                float distanceSq = Vector3.SqrMagnitude(checkPosition - unitPosition);

                if (distanceSq <= closestDistanceSq && distanceSq <= unit.UnitData.Radius)
                {
                    closestDistanceSq = distanceSq;
                    focusedUnit = unit;
                }
            }

            selectableController.UnitFocusedPoint(focusedUnit);
        }

        public void CheckAreaFocused(Vector3 standardPosition, float width, float height)
        {                        
            var units = spatialHash.GetUnitsInRange(standardPosition, width, height);
            selectableController.UnitFocusedList(units);
        }

        public void RightClickMove(Vector3 destination)
        {
            selectableController.RightClickMove(destination);
        }
        public void ShiftRightClickMove(Vector3 destination)
        {
            selectableController.ShiftRightClickMove(destination);
        }
    }
}
