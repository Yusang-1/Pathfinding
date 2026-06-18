using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public class SelectableController
    {
        private SelectableType currentSelectedType;
        private readonly HashSet<ISelectableUnit> currentSelectedList = new();

        private Action<string> OnchangeActionMapSelected;
        private Action OnchangeActionMapDefault;
        
        public void GetActions(Action<string> changeActionMapSelected, Action changeActionMapDefault)
        {
            OnchangeActionMapSelected = changeActionMapSelected;
            OnchangeActionMapDefault = changeActionMapDefault;
        }

        public void Selected(ISelectableUnit selectable)
        {
            if (selectable == null)
            {
                DeselectedAll();
                return;
            }

            NewSelected(selectable);
        }
        public void SelectedList(ICollection<ISelectableUnit> selectableList)
        {
            if(selectableList == null || selectableList.Count == 0) return;
            
            int count = 0;
            foreach (var selectable in selectableList)
            {
                if (selectable == null)
                {
                    continue;
                }
                
                if(count == 0)
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
        public void ShiftSelected(ISelectableUnit selectable)
        {
            if (selectable == null) return;

            if (currentSelectedList.Contains(selectable)) // 이미 선택중이면
            {
                Deselected(selectable);
            }
            else if (currentSelectedType == selectable.GetSelectableType()) // 현재 타입과 같으면
            {
                AddSelected(selectable);
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
            selectable.Selected();
            currentSelectedList.Add(selectable);
        }

        public void Deselected(ISelectableUnit selectable)
        {
            selectable.Deselected();
            currentSelectedList.Remove(selectable);

            if (currentSelectedList.Count == 0)
            {
                currentSelectedType = SelectableType.None;
                OnchangeActionMapDefault?.Invoke();
            }
        }
        private void DeselectedAll()
        {
            if (currentSelectedList.Count == 0) return;

            foreach (var selected in currentSelectedList)
            {
                selected.Deselected();
            }
            currentSelectedList.Clear();

            currentSelectedType = SelectableType.None;
            OnchangeActionMapDefault?.Invoke();
        }
        
        public void UnitFocused(List<ISelectableUnit> selectables)
        {
            if(selectables == null || selectables.Count == 0) return;
            
            foreach(var selectable in selectables)
            {
                UnitFocused(selectable);
            }
        }
        public void UnitFocused(ISelectableUnit selectable)
        {
            selectable.Focused();
        }
        
        public void UnitUnfocused(List<ISelectableUnit> selectables)
        {
            if(selectables == null || selectables.Count == 0) return;
            
            foreach(var selectable in selectables)
            {
                UnitUnfocused(selectable);
            }
        }
        public void UnitUnfocused(ISelectableUnit selectable)
        {
            selectable.Unfocused();
        }
    }    
}
