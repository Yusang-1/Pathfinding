using System;

namespace Assets.Scripts.CreateMap
{
    public class SelectableController
    {
        private ISelectable currentSelected;

        public void Selected(ISelectable selectable)
        {
            if(currentSelected == selectable) return;
            
            currentSelected?.Deselected();

            currentSelected = selectable;

            currentSelected?.Selected();
        }
        public void Deselected(ISelectable selectable)
        {
            currentSelected?.Deselected();
            currentSelected = null;
        }
    }

    // public interface ISelectable
    // {
    //     public event Action<ISelectable> OnSelectedCallback;
    //     public event Action<ISelectable> OnDeselectedCallback;

    //     public void Selected();
    //     public void Deselected();
    // }
}
