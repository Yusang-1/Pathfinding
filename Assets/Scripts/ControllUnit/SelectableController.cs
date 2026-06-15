using System;

namespace Assets.Scripts.ControllUnit
{
    public class SelectableController
    {
        private ISelectable currentSelected;

        private readonly Action<string> enableActionMap;
        private readonly Action disableActionMap;

        public SelectableController(Action<string> enableActionMap, Action disableActionMap)
        {
            this.enableActionMap = enableActionMap;
            this.disableActionMap = disableActionMap;
        }

        public void Selected(ISelectable selectable)
        {
            if(currentSelected == selectable) return;
            
            if (currentSelected != null)
            {
                currentSelected.Deselected();
                if (currentSelected is IHaveOwnActionMap)
                {
                    (currentSelected as IHaveOwnActionMap).OnEnableActionMap -= enableActionMap;
                    (currentSelected as IHaveOwnActionMap).OnDisableActionMap -= disableActionMap;
                }
            }

            currentSelected = selectable;
            if (currentSelected is IHaveOwnActionMap)
            {
                (currentSelected as IHaveOwnActionMap).OnEnableActionMap += enableActionMap;
                (currentSelected as IHaveOwnActionMap).OnDisableActionMap += disableActionMap;
            }

            currentSelected?.Selected();
        }
        public void Deselected(ISelectable selectable)
        {
            currentSelected?.Deselected();
            currentSelected = null;
        }
    }

    public interface ISelectableUnit
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;

        public void Selected();
        public void Deselected();
    }
}
