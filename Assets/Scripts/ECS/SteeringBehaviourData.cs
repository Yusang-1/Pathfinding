using Unity.Entities;

namespace Assets.Scripts.ECS
{
    public struct SteeringBehaviourData : IComponentData
    {
        public float SeekWeight;
        public float SeparationWeight;
        public float AlignmentWeight;
        public float CohesionWeight;
    }
}
