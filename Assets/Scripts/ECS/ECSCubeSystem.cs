using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct ECSCubeSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var elasped = (float)SystemAPI.Time.ElapsedTime;
        
        // SystemAPI의 쿼리를 사용해 ECSCube와 LocalTransform을 모두 갖는 엔티티를 열거
        foreach(var (ecsCube, transform) in SystemAPI.Query<RefRO<ECSCube>, RefRW<LocalTransform>>())
        {
            var t = ecsCube.ValueRO.Speed * elasped;
            var y = math.abs(math.sin(t)) * 0.1f;
            var bank = math.cos(t) * 0.5f;
            
            var fwd = transform.ValueRO.Forward();
            var rot = quaternion.AxisAngle(fwd, bank);
            var up = math.mul(rot, math.float3(0,1,0));
            
            transform.ValueRW.Position.y = y;
            transform.ValueRW.Rotation = quaternion.LookRotation(fwd, up);
        }
    }
}
