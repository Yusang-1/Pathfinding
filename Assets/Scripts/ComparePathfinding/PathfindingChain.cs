using UnityEngine;
using System.Collections.Generic;

public class PathfindingChain
{
    public ProcessorDelegate<(Vector3, Vector3), List<Vector3>> GetHPAStarSmooth(HPAPathfinder hPAFinder, ClusterPathSmoother pathSmoother, SearchWithTheClusterResult searchWithTheCluster)
    {
        var hpaPathfinder = FindClusterPath(hPAFinder).SmoothHPAPath(pathSmoother)
            .Then(new SearchWithClusterResultProcessor(searchWithTheCluster))
            .Compile();
            
        return hpaPathfinder;
    }    

    private FindClusterPathChain FindClusterPath(HPAPathfinder hPAPathfinder) => new FindClusterPathChain(new FindClusterPathProcessor(hPAPathfinder));
}
