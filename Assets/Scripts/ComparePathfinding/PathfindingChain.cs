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
    
    public void Initialize(HPAPathfinder hPAFinder, ClusterPathSmoother pathSmoother, SearchWithTheClusterResult searchWithTheCluster)
    {
        ClusterPath_StringPulling = CreateCLusterPathSmooth(hPAFinder, pathSmoother);

        HPAStar_StringPulling = CreateHPAStarSmooth(hPAFinder, pathSmoother, searchWithTheCluster);

        HPAStar_StringPulling_Theta = CreateHPAStarSmoothTheta(hPAFinder, pathSmoother, searchWithTheCluster);
        
        HPAStar_Theta = CreateHPATheta(hPAFinder, searchWithTheCluster);
    }

    private ProcessorDelegate<(Vector3, Vector3), List<ClusterResult>> CreateCLusterPathSmooth(HPAPathfinder hPAFinder, ClusterPathSmoother pathSmoother)
    {
        var clusterPathfinder = FindClusterPath(hPAFinder).SmoothHPAPath(pathSmoother)
            .Compile();

        return clusterPathfinder;
    }

    private ProcessorDelegate<(Vector3, Vector3), List<Vector3>> CreateHPAStarSmooth(HPAPathfinder hPAFinder, ClusterPathSmoother pathSmoother, SearchWithTheClusterResult searchWithTheCluster)
    {
        var hpaPathfinder = FindClusterPath(hPAFinder).SmoothHPAPath(pathSmoother)
            .Then(new SearchWithClusterResultProcessor(searchWithTheCluster.FindPath))
            .Compile();

        return hpaPathfinder;
    }

    private ProcessorDelegate<(Vector3, Vector3), List<Vector3>> CreateHPAStarSmoothTheta(HPAPathfinder hPAFinder, ClusterPathSmoother pathSmoother, SearchWithTheClusterResult searchWithTheCluster)
    {
        var hpaPathfinder = FindClusterPath(hPAFinder).SmoothHPAPath(pathSmoother)
            .Then(new SearchWithClusterResultProcessor(searchWithTheCluster.FindSmoothPathTheta))
            .Compile();

        return hpaPathfinder;
    }
    
    private ProcessorDelegate<(Vector3, Vector3), List<Vector3>> CreateHPATheta(HPAPathfinder hPAFinder, SearchWithTheClusterResult searchWithTheCluster)
    {
        var hpaPathfinder = FindClusterPath(hPAFinder)
            .Then(new SearchWithClusterResultProcessor(searchWithTheCluster.FindPathTheta))
            .Compile();

        return hpaPathfinder;
    }

    public FindClusterPathChain FindClusterPath(HPAPathfinder hPAPathfinder) => new FindClusterPathChain(new FindClusterPathProcessor(hPAPathfinder));
}
