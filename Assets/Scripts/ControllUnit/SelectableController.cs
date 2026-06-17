using System;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class SelectableController
    {
        private SelectableType currentSelectedType;
        private readonly HashSet<ISelectableUnit> currentSelectedList = new();

        private readonly Action<string> OnchangeActionMapSelected;
        private readonly Action OnchangeActionMapDefault;

        public SelectableController(Action<string> changeActionMapSelected, Action changeActionMapDefault)
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
        public void SelectedList(List<ISelectableUnit> selectableList)
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
    }

    public interface ISelectableUnit
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;

        public void Selected();
        public void Deselected();
        public SelectableType GetSelectableType();
    }

    public enum SelectableType
    {
        None,
        Unit
    }
}
