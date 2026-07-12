public class PathResultRecorder
{
    static PathResult result = new();

    static public void ResetPathResult() => result.Reset();
    static public void ResetPathLength() => result.PathLength = 0;
    static public void AddSearchedCount() => result.SearchedCount++;
    static public void AddPathLength(float length) => result.PathLength += length;
    static public void AddMemoryUsed(int used) => result.MemoryUsed += used;
    
    static public float GetPathLength() => result.PathLength;
    static public PathResult GetPathResult() => result;

    public struct PathResult
    {
        public int SearchedCount;
        public float PathLength;
        public int MemoryUsed;

        public void Reset()
        {
            SearchedCount = 0;
            PathLength = 0;
            MemoryUsed = 0;
        }
    }
}
