using UnityEngine;
using System.Collections.Generic;

public class PathfindingChain
{
    /// <summary> String Pulling을 적용한 ClusterPath를 반환 </summary>
    public ProcessorDelegate<(Vector3, Vector3), List<ClusterResult>> ClusterPath_StringPulling { get; private set; }

    /// <summary> String Pulling을 적용한 ClusterPath + AStar </summary>
    public ProcessorDelegate<(Vector3, Vector3), List<Vector3>> HPAStar_StringPulling { get; private set; }

    /// <summary> String Pulling을 적용한 ClusterPath + ThetaStar </summary>
    public ProcessorDelegate<(Vector3, Vector3), List<Vector3>> HPAStar_StringPulling_Theta { get; private set; }

    /// <summary> ClusterPath + ThetaStar </summary>
    public ProcessorDelegate<(Vector3, Vector3), List<Vector3>> HPAStar_Theta { get; private set; }

    public void Initialize(HPAPathfinder hPAFinder, ClusterPathSmoother pathSmoother, SearchWithTheClusterResult searchWithTheCluster, float unitRadius)
    {
        ClusterPath_StringPulling = CreateCLusterPathSmooth(hPAFinder, pathSmoother, unitRadius);

        HPAStar_StringPulling = CreateHPAStarSmooth(hPAFinder, pathSmoother, searchWithTheCluster, unitRadius);

        HPAStar_StringPulling_Theta = CreateHPAStarSmoothTheta(hPAFinder, pathSmoother, searchWithTheCluster, unitRadius);

        HPAStar_Theta = CreateHPATheta(hPAFinder, searchWithTheCluster, unitRadius);
    }

    private ProcessorDelegate<(Vector3, Vector3), List<ClusterResult>> CreateCLusterPathSmooth(HPAPathfinder hPAFinder, ClusterPathSmoother pathSmoother, float unitRadius)
    {
        var clusterPathfinder = FindClusterPath(hPAFinder, unitRadius).SmoothHPAPath(pathSmoother, unitRadius)
            .Compile();

        return clusterPathfinder;
    }

    private ProcessorDelegate<(Vector3, Vector3), List<Vector3>> CreateHPAStarSmooth(HPAPathfinder hPAFinder, ClusterPathSmoother pathSmoother, SearchWithTheClusterResult searchWithTheCluster, float unitRadius)
    {
        var hpaPathfinder = FindClusterPath(hPAFinder, unitRadius).SmoothHPAPath(pathSmoother, unitRadius)
            .Then(new SearchWithClusterResultProcessor(searchWithTheCluster.FindPath))
            .Compile();

        return hpaPathfinder;
    }

    private ProcessorDelegate<(Vector3, Vector3), List<Vector3>> CreateHPAStarSmoothTheta(HPAPathfinder hPAFinder, ClusterPathSmoother pathSmoother, SearchWithTheClusterResult searchWithTheCluster, float unitRadius)
    {
        var hpaPathfinder = FindClusterPath(hPAFinder, unitRadius).SmoothHPAPath(pathSmoother, unitRadius)
            .Then(new SearchWithClusterResultProcessor(searchWithTheCluster.FindSmoothPathTheta))
            .Compile();

        return hpaPathfinder;
    }

    private ProcessorDelegate<(Vector3, Vector3), List<Vector3>> CreateHPATheta(HPAPathfinder hPAFinder, SearchWithTheClusterResult searchWithTheCluster, float unitRadius)
    {
        var hpaPathfinder = FindClusterPath(hPAFinder, unitRadius)
            .Then(new SearchWithClusterResultProcessor(searchWithTheCluster.FindPathTheta))
            .Compile();

        return hpaPathfinder;
    }

    public FindClusterPathChain FindClusterPath(HPAPathfinder hPAPathfinder, float unitRadius)
    {
        return new FindClusterPathChain(new FindClusterPathProcessor(hPAPathfinder, unitRadius));
    }
}
