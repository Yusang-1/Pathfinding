using System;

namespace Assets.Scripts.ControllUnit
{
    public interface ISelectableUnit
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;

        public void Selected();
        public void Deselected();
        public void Focused();
        public void Unfocused();
        public SelectableType GetSelectableType();
    }

    public enum SelectableType
    {
        None,
        Unit
    }
}

