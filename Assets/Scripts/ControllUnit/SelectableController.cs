using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class SelectableController
    {
        private readonly SlotDestination slotDestination = new();

        private SelectableType currentSelectedType;
        private readonly HashSet<ISelectableUnit> currentSelectedHash = new();
        private readonly HashSet<ISelectableUnit> alreadyFocusedHash = new();
        private readonly List<ISelectableUnit> unfocusedUnits = new();
        private readonly List<ISelectableUnit> newlyFocusedUnit = new();

        private Action<string> OnchangeActionMapSelected;
        private Action OnchangeActionMapDefault;

        public void GetActions(Action<string> changeActionMapSelected, Action changeActionMapDefault)
        {
            OnchangeActionMapSelected = changeActionMapSelected;
            OnchangeActionMapDefault = changeActionMapDefault;
        }

        public void Selected()
        {
            if (alreadyFocusedHash == null || alreadyFocusedHash.Count == 0)
            {
                DeselectedAll();
                return;
            }

            SelectedList(alreadyFocusedHash);
        }
        private void SelectedList(ICollection<ISelectableUnit> selectableList)
        {
            if (selectableList == null || selectableList.Count == 0)
            {
                DeselectedAll();
                return;
            }

            int count = 0;
            foreach (var selectable in selectableList)
            {
                if (selectable == null)
                {
                    continue;
                }

                if (count == 0)
                {
                    NewSelected(selectable);
                }
                else
                {
                    AddSelected(selectable);
                }
                count++;
            }
        }
        public void ShiftSelected()
        {
            if (alreadyFocusedHash == null || alreadyFocusedHash.Count == 0) return;

            foreach (var selectable in alreadyFocusedHash)
            {
                if (currentSelectedHash.Contains(selectable)) // 이미 선택중이면
                {
                    Deselected(selectable);
                }
                else if (currentSelectedType == selectable.GetSelectableType()) // 현재 타입과 같으면
                {
                    AddSelected(selectable);
                }
            }
        }
        public void ShiftSelectedList()
        {
            if (alreadyFocusedHash == null || alreadyFocusedHash.Count == 0) return;

            foreach (var selectable in alreadyFocusedHash)
            {
                if (currentSelectedType == selectable.GetSelectableType()) // 현재 타입과 같으면
                {
                    AddSelected(selectable);
                }
            }
        }

        private void NewSelected(ISelectableUnit selectable)
        {
            DeselectedAll();
            AddSelected(selectable);
            currentSelectedType = selectable.GetSelectableType();

            if (selectable is IHaveOwnActionMap)
            {
                var actionMapOwner = selectable as IHaveOwnActionMap;
                OnchangeActionMapSelected?.Invoke(actionMapOwner.GetActionMapName());
            }
        }
        private void AddSelected(ISelectableUnit selectable)
        {
            if (currentSelectedHash.Contains(selectable)) return;

            selectable.Selected();
            currentSelectedHash.Add(selectable);
        }

        private void Deselected(ISelectableUnit selectable)
        {
            selectable.Deselected();
            currentSelectedHash.Remove(selectable);

            if (currentSelectedHash.Count == 0)
            {
                currentSelectedType = SelectableType.None;
                OnchangeActionMapDefault?.Invoke();
            }
        }
        private void DeselectedAll()
        {
            if (currentSelectedHash.Count == 0) return;

            foreach (var selected in currentSelectedHash)
            {
                selected.Deselected();
            }
            currentSelectedHash.Clear();

            currentSelectedType = SelectableType.None;
            OnchangeActionMapDefault?.Invoke();
        }

        public void UnitFocusedList(HashSet<ISelectableUnit> selectables)
        {
            if (selectables == null || selectables.Count == 0) return;

            var unfocusedUnits = FindUnfocusedUnits(alreadyFocusedHash, selectables);
            var newlyFocusedUnit = FindNewlyFocusedUnits(alreadyFocusedHash, selectables);

            UnitUnfocusedList(unfocusedUnits);

            foreach (var newlyFocused in newlyFocusedUnit)
            {
                UnitFocused(newlyFocused);
            }
        }
        public void UnitFocusedPoint(ISelectableUnit selectable)
        {
            if (selectable == null)
            {
                UnfocusedAll();
                return;
            }
            if (alreadyFocusedHash.Contains(selectable)) return;

            UnitFocused(selectable);
        }
        private void UnitFocused(ISelectableUnit selectable)
        {
            if (selectable == null) return;

            selectable.Focused();
            alreadyFocusedHash.Add(selectable);
        }
        private void UnfocusedAll()
        {
            if (alreadyFocusedHash == null || alreadyFocusedHash.Count == 0) return;

            foreach (var focused in alreadyFocusedHash)
            {
                focused.Unfocused();
            }
            alreadyFocusedHash.Clear();
        }

        private void UnitUnfocusedList(List<ISelectableUnit> selectables)
        {
            if (selectables == null || selectables.Count == 0) return;

            foreach (var selectable in selectables)
            {
                UnitUnfocused(selectable);
            }
        }
        private void UnitUnfocused(ISelectableUnit selectable)
        {
            selectable.Unfocused();
            alreadyFocusedHash.Remove(selectable);
        }

        private List<ISelectableUnit> FindUnfocusedUnits(HashSet<ISelectableUnit> alreadyFocused, HashSet<ISelectableUnit> newFocused)
        {
            if (alreadyFocused == null || alreadyFocused.Count == 0) return null;
            if (newFocused == null || newFocused.Count == 0)
            {
                // alreadyFocused Hash의 요소를 unfocusedUnits List에 복사해 리턴
                unfocusedUnits.Clear();
                foreach (var focused in alreadyFocused)
                {
                    unfocusedUnits.Add(focused);
                }
                return unfocusedUnits;
            }

            unfocusedUnits.Clear();

            foreach (var focused in alreadyFocused)
            {
                if (!newFocused.Contains(focused))
                {
                    unfocusedUnits.Add(focused);
                }
            }

            return unfocusedUnits;
        }

        private List<ISelectableUnit> FindNewlyFocusedUnits(HashSet<ISelectableUnit> alreadyFocused, HashSet<ISelectableUnit> newFocused)
        {
            if (newFocused == null || newFocused.Count == 0) return null;
            if (alreadyFocused == null || alreadyFocused.Count == 0)
            {
                // newFocused Hash의 요소를 newlyFocusedUnit List에 복사해 리턴
                newlyFocusedUnit.Clear();
                foreach (var focused in newFocused)
                {
                    newlyFocusedUnit.Add(focused);
                }
                return newlyFocusedUnit;
            }

            newlyFocusedUnit.Clear();

            foreach (var newFocus in newFocused)
            {
                if (!alreadyFocused.Contains(newFocus))
                {
                    newlyFocusedUnit.Add(newFocus);
                }
            }

            return newlyFocusedUnit;
        }

        public void RightClickMove(Vector3 destination)
        {
            foreach (var unit in currentSelectedHash)
            {
                Vector3 newDestination = slotDestination.GetSlotDestination(unit as Unit, destination, currentSelectedHash.Count);
                (unit as Unit).Controller.MoveTo(newDestination, currentSelectedHash.Count);
            }
        }
        public void ShiftRightClickMove(Vector3 destination)
        {
            foreach (var unit in currentSelectedHash)
            {
                Vector3 newDestination = slotDestination.GetSlotDestination(unit as Unit, destination, currentSelectedHash.Count);
                (unit as Unit).Controller.MoveToReservation(newDestination, currentSelectedHash.Count);
            }
        }
    }
}
