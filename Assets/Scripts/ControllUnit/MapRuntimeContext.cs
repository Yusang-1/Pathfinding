namespace Assets.Scripts.ControllUnit
{
    public class MapRuntimeContext
    {
        private readonly NodeList nodeList = new();
        private readonly SpatialHash spatialHash = new();

        public NodeList NodeList => nodeList;
        public SpatialHash SpatialHash => spatialHash;
    }
}
