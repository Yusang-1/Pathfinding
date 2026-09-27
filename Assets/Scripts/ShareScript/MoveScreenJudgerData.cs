using UnityEngine;

[CreateAssetMenu(fileName = "MoveScreenJudgerData", menuName = "Scriptable Objects/MoveScreenJudgerData")]
public class MoveScreenJudgerData : ScriptableObject
{
    // 화면 이동 구역의 비율
    [SerializeField] private float areaRatio;
    [SerializeField] private float curveAreaRatio;
    [SerializeField] private float maxSpeed;
        
    public float MinMoveAreaValue => areaRatio;
    public float MaxMoveAreaValue => 1 - areaRatio;
    public float MinCurveAreaValue => curveAreaRatio;
    public float MaxCurveAreaValue => 1 - curveAreaRatio;
    public float MaxSpeed => maxSpeed;
}
