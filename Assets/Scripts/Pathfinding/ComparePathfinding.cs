using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Pathfinding
{
    public class ComparePathfinding
    {
        public event Action<bool> OnPathFound;
        public event Action<PathResultRecorder.PathResult> OnAFound;
        public event Action<PathResultRecorder.PathResult> OnHPASmoothAStarFound;
        public event Action<PathResultRecorder.PathResult> OnHPAThetaFound;
        public event Action<PathResultRecorder.PathResult> OnHPASmoothThetaFound;

        private readonly NodeList nodeList;
        private readonly HPAClusterList clusterList;
        private readonly AStarPathfinder aStarPathfinder;

        private readonly ClusterResultWrapper clusterResultWrapper;
        private readonly PathfindingChain pathfindingChain;

        public ClusterResultWrapper CurrentAbstractResults { get; private set; }

        public Dictionary<NodeType, List<Vector2Int>> AStarResult { get; private set; } = new();
        public Dictionary<NodeType, List<Vector2Int>> HpaStarResult { get; private set; } = new();
        public Dictionary<NodeType, List<Vector2Int>> HpaThetaResult { get; private set; } = new();
        public Dictionary<NodeType, List<Vector2Int>> HpaStarSmoothResult { get; private set; } = new();
        public List<Vector3> SmoothPath { get; private set; }
        private Vector3 from, to;

        public ComparePathfinding(NodeList nodeList, HPAClusterList clusterList, AStarPathfinder aStarPathfinder, ClusterResultWrapper clusterResultWrapper, PathfindingChain pathfindingChain)
        {
            this.nodeList = nodeList;
            this.clusterList = clusterList;
            this.aStarPathfinder = aStarPathfinder;
            this.clusterResultWrapper = clusterResultWrapper;
            this.pathfindingChain = pathfindingChain;
        }

        public void DoComparePathfinding(Vector3 from, Vector3 to)
        {
            this.from = from;
            this.to = to;

            AStarResult = FindAStarPath();

            HpaStarResult = FindHPA_Smoothing_AStarPath();

            HpaThetaResult = FindHPA_ThetaPath();

            HpaStarSmoothResult = FindHPA_Smoothing_ThetaPath();

            nodeList.NodeTypeController.NodeTypeDrawer.IsDuringNodeSetting = false;
            OnPathFound?.Invoke(true);
        }

        private Dictionary<NodeType, List<Vector2Int>> FindAStarPath()
        {
            clusterList.SetAllCLusterActive();
            PathResultRecorder.ResetPathResult();

            var path = aStarPathfinder.FindPath(from, to, 0);
            Vector3ListPool.ReleaseValue(path);

            OnAFound?.Invoke(PathResultRecorder.GetPathResult());

            var result = nodeList.NodeTypeController.NodeTypeDrawer.GetNodeInfo();
            nodeList.NodeTypeController.NodeTypeDrawer.ClearDict();
            return result;
        }

        private readonly float tempUnitRadius = 0;
        private Dictionary<NodeType, List<Vector2Int>> FindHPA_Smoothing_AStarPath()
        {
            clusterList.ResetClusterList();
            PathResultRecorder.ResetPathResult();

            clusterResultWrapper.Reset();
            clusterResultWrapper.SetStart(from, to, tempUnitRadius);

            CurrentAbstractResults = pathfindingChain.ClusterPath_StringPulling?.Invoke(clusterResultWrapper);

            pathfindingChain.HPAStar_StringPulling?.Invoke(clusterResultWrapper);

            OnHPASmoothAStarFound?.Invoke(PathResultRecorder.GetPathResult());

            var result = nodeList.NodeTypeController.NodeTypeDrawer.GetNodeInfo();
            nodeList.NodeTypeController.NodeTypeDrawer.ClearDict();
            return result;
        }

        private Dictionary<NodeType, List<Vector2Int>> FindHPA_ThetaPath()
        {
            clusterList.ResetClusterList();
            PathResultRecorder.ResetPathResult();

            clusterResultWrapper.Reset();
            clusterResultWrapper.SetStart(from, to, tempUnitRadius);

            pathfindingChain.HPAStar_Theta?.Invoke(clusterResultWrapper);

            OnHPAThetaFound?.Invoke(PathResultRecorder.GetPathResult());

            var result = nodeList.NodeTypeController.NodeTypeDrawer.GetNodeInfo();
            result[NodeType.trace].Add(nodeList.GetNodeIndex(to));

            nodeList.NodeTypeController.NodeTypeDrawer.ClearDict();
            return result;
        }

        private Dictionary<NodeType, List<Vector2Int>> FindHPA_Smoothing_ThetaPath()
        {
            clusterList.ResetClusterList();
            PathResultRecorder.ResetPathResult();

            clusterResultWrapper.Reset();
            clusterResultWrapper.SetStart(from, to, tempUnitRadius);

            SmoothPath = pathfindingChain.HPAStar_StringPulling_Theta?.Invoke(clusterResultWrapper);

            OnHPASmoothThetaFound?.Invoke(PathResultRecorder.GetPathResult());

            var result = nodeList.NodeTypeController.NodeTypeDrawer.GetNodeInfo();
            nodeList.NodeTypeController.NodeTypeDrawer.ClearDict();
            return result;
        }
    }
}
