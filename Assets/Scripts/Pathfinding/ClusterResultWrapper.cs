using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public class ClusterResultWrapper
    {
        public Vector3 From { get; private set; }
        public Vector3 To { get; private set; }
        public float UnitRadius { get; private set; }

        public List<ClusterSmootherResult> ClusterSmootherResult { get; private set; } = new();
        public List<ClusterResult> ClusterResults { get; private set; } = new();

        public void SetStart(Vector3 from, Vector3 to, float unitRadius)
        {
            this.From = from;
            this.To = to;
            this.UnitRadius = unitRadius;
        }

        public void ResetAll()
        {
            ResetClusterResult();
            ResetSmootherClusterResult();
        }

        public void ResetClusterResult()
        {
            ClusterResults.Clear();
        }

        public void ResetSmootherClusterResult()
        {
            ClusterSmootherResult.Clear();
        }

        public void SetClusterResult(List<ClusterResult> results)
        {
            ClusterResults = results;
        }
        public void SetClusterResult(ClusterResult result)
        {
            ClusterResults.Add(result);
        }

        public void SetClusterSmootherResult(List<ClusterSmootherResult> smootherResults)
        {
            ClusterSmootherResult = smootherResults;
        }
        public void SetClusterSmootherResult(ClusterSmootherResult smootherResult)
        {
            ClusterSmootherResult.Add(smootherResult);
        }
    }

    public class ClusterSmootherResult : IEquatable<ClusterSmootherResult>
    {
        public List<Vector2Int> ClusterIndexes = new();
        public Vector2Int EnterNodeIndex;
        public Vector2Int ExitNodeIndex;

        public void SetSmootherResult(List<Vector2Int> clusters, Vector2Int exitIndex, Vector2Int startIndex, Vector2Int notIncludeClusterIndex, bool useNotIncludeClusterIndex)
        {
            ClusterIndexes.Clear();
            for (int i = 0; i < clusters.Count; i++)
            {
                if (useNotIncludeClusterIndex && clusters[i] == notIncludeClusterIndex) continue;

                ClusterIndexes.Add(clusters[i]);
            }
            EnterNodeIndex = startIndex;
            ExitNodeIndex = exitIndex;
        }

        public void SetData(List<Vector2Int> clusters, Vector2Int exitIndex, Vector2Int startIndex)
        {
            ClusterIndexes.Clear();
            for (int i = 0; i < clusters.Count; i++)
            {
                ClusterIndexes.Add(clusters[i]);
            }
            EnterNodeIndex = startIndex;
            ExitNodeIndex = exitIndex;
        }

        public void Clear()
        {
            ClusterIndexes.Clear();
        }

        public override int GetHashCode()
        {
            int listHash = 17;
            if (ClusterIndexes != null)
            {
                unchecked
                {
                    foreach (var point in ClusterIndexes)
                    {
                        // 각 Vector2Int의 해시코드를 순차적으로 조합
                        listHash = listHash * 23 + point.GetHashCode();
                    }
                }
            }

            return HashCode.Combine(listHash, EnterNodeIndex, ExitNodeIndex);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ClusterSmootherResult);
        }

        public bool Equals(ClusterSmootherResult other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true; // 같은 참조면 true

            // 일반 필드 비교
            if (EnterNodeIndex != other.EnterNodeIndex || ExitNodeIndex != other.ExitNodeIndex) return false;

            // 리스트 필드 비교
            if (ClusterIndexes == other.ClusterIndexes) return true; // 둘 다 null이거나 같은 참조인 경우
            if (ClusterIndexes is null || other.ClusterIndexes is null) return false; // 둘 중 하나가 null인 경우

            // 리스트 내부의 Vector2Int 값과 순서가 일치하는지 비교
            return ClusterIndexes.SequenceEqual(other.ClusterIndexes);
        }

        // 비교 연산자(==, !=) 오버라이드
        public static bool operator ==(ClusterSmootherResult left, ClusterSmootherResult right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }

        public static bool operator !=(ClusterSmootherResult left, ClusterSmootherResult right)
        {
            return !(left == right);
        }
    }

    public struct ClusterResult
    {
        public Vector2Int Index;
        public Vector2Int EnterDirection;
        public Vector2Int ExitDirection;
        public Vector2Int EntranceExit;
    }
}
