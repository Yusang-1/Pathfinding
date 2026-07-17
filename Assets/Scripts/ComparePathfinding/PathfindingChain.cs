using UnityEngine;
using System.Collections.Generic;

public class PathfindingChain
{
    public ProcessorDelegate<(Vector3, Vector3), List<ClusterResult>> ClusterPath_StringPulling { get; private set; }
    public ProcessorDelegate<(Vector3, Vector3), List<Vector3>> HPAStar_StringPulling { get; private set; }
    public ProcessorDelegate<(Vector3, Vector3), List<Vector3>> HPAStar_StringPulling_Theta { get; private set; }

    public void Initialize(HPAPathfinder hPAFinder, ClusterPathSmoother pathSmoother, SearchWithTheClusterResult searchWithTheCluster)
    {
        ClusterPath_StringPulling = CreateCLusterPathSmooth(hPAFinder, pathSmoother);
        
        HPAStar_StringPulling = CreateHPAStarSmooth(hPAFinder, pathSmoother, searchWithTheCluster);
        
        HPAStar_StringPulling_Theta = CreateHPAStarSmoothTheta(hPAFinder, pathSmoother, searchWithTheCluster);
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
            .Then(new SearchWithClusterResultProcessor(searchWithTheCluster.FindPathTheta))
            .Compile();

        return hpaPathfinder;
    }

    public FindClusterPathChain FindClusterPath(HPAPathfinder hPAPathfinder) => new FindClusterPathChain(new FindClusterPathProcessor(hPAPathfinder));
}
