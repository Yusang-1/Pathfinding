public class SelectableController
{
    private ISelectable currentSelected;
    
    public void Selected(ISelectable selectable)
    {
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

public interface ISelectable
{    
    public void Selected();
    public void Deselected();
    
}
