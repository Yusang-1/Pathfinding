using Unity.Mathematics;

namespace Assets.Scripts.ECSControllUnit
{
    public static class SpatialHashUtility
    {
        private const float GRID_SIZE = 1;
        private const float Cell_SIZE = 2;
        private const int HASH_KEY_X = 73856093;
        private const int HASH_KEY_Y = 19349663;

        public static int2 GetCell(float3 position)
        {
            position.x += GRID_SIZE / 2;
            position.y += GRID_SIZE / 2;

            return (int2)math.floor(position.xy / Cell_SIZE);
        }

        public static int GetHash(int2 cell)
        {
            unchecked // 정수형 연산이나 변환 과정에서 오버플로우(Overflow)나 언더플로우(Underflow)가 발생하더라도 예외를 발생시키지 않고 무시하도록 지정
            {
                return cell.x * HASH_KEY_X ^ cell.y * HASH_KEY_Y;
            }
        }

        public static int GetHash(float3 position)
        {
            return GetHash(GetCell(position));
        }
    }
}
