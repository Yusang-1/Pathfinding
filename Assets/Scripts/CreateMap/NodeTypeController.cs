namespace Assets.Scripts.CreateMap
{
    public class NodeTypeController
    {
        private NodeType currentSelectedType;
        private NodeList nodeList;

        public void Initialize(NodeList nodeList)
        {
            this.nodeList = nodeList;
        }

        public void SetCurrentSelected(NodeType type)
        {
            currentSelectedType = type;
        }

        public void SetNodeType(ISelectable selectable)
        {
            if (currentSelectedType == default) currentSelectedType = NodeType.obstacle;

            nodeList.SetNodeType((selectable as Node).Index, currentSelectedType);
        }
    }
}
