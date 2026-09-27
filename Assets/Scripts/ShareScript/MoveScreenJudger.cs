using UnityEngine;

public class MoveScreenJudger : MonoBehaviour
{
    [SerializeField] private MoveScreenJudgerData data;

    public bool TryGetScreenMoveVelocity(Vector2 inputViewportPosition, out Vector2 velocity)
    {
        if (IsInScreenMoveArea(inputViewportPosition))
        {
            velocity = GetMoveVelocity(inputViewportPosition);
            return true;
        }
        else
        {
            velocity = Vector2.zero;
            return false;
        }
    }

    private bool IsInScreenMoveArea(Vector2 inputViewportPosition)
    {
        float inputX = inputViewportPosition.x;
        float inputY = inputViewportPosition.y;

        bool isXInMoveArea = inputX <= data.MinMoveAreaValue || inputX >= data.MaxMoveAreaValue;
        bool isYInMoveArea = inputY <= data.MinMoveAreaValue || inputY >= data.MaxMoveAreaValue;

        if (isXInMoveArea || isYInMoveArea)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Vector2 GetMoveVelocity(Vector2 inputViewportPosition)
    {
        float inputX = inputViewportPosition.x;
        float inputY = inputViewportPosition.y;

        bool isXInRightCurveArea = inputX >= data.MaxCurveAreaValue;
        bool isXInLeftCurveArea = inputX <= data.MinCurveAreaValue;

        bool isYInUpCurveArea = inputY >= data.MaxCurveAreaValue;
        bool isYInDownCurveArea = inputY <= data.MinCurveAreaValue;

        Vector2 velocity;
        // curve 판정 안에 있는 경우        
        if ((isXInRightCurveArea || isXInLeftCurveArea) && (isYInUpCurveArea || isYInDownCurveArea))
        {
            velocity = GetCurveDirection(inputViewportPosition, isXInRightCurveArea, isYInDownCurveArea);
        }
        else
        {
            velocity = GetStraightVelocity(inputX, inputY);
        }

        return velocity;
    }

    private Vector2 GetStraightVelocity(float inputX, float inputY)
    {
        bool isXInMoveArea = inputX <= data.MinMoveAreaValue || inputX >= data.MaxMoveAreaValue;

        bool isXInRightStraightArea = inputX >= data.MaxMoveAreaValue;
        bool isYInUpStraightArea = inputY >= data.MaxMoveAreaValue;

        Vector2 direction;
        Vector2 velocity;
        float percent;
        float speed;

        if (isXInMoveArea)
        {
            if (isXInRightStraightArea) // 우측
            {
                direction = Vector2.right;

                percent = (inputX - data.MaxMoveAreaValue) / data.MinMoveAreaValue;
            }
            else // 좌측
            {
                direction = Vector2.left;

                percent = (data.MinMoveAreaValue - inputX) / data.MinMoveAreaValue;
            }
        }
        else
        {
            if (isYInUpStraightArea) // 상단
            {
                direction = Vector2.up;

                percent = (inputY - data.MaxMoveAreaValue) / data.MinMoveAreaValue;
            }
            else // 하단
            {
                direction = Vector2.down;

                percent = (data.MinMoveAreaValue - inputY) / data.MinMoveAreaValue;
            }
        }
        
        if(percent > 1) percent = 1;
        speed = percent * data.MaxSpeed;
        velocity = speed * Time.deltaTime * direction;

        return velocity;
    }

    private Vector2 GetCurveDirection(Vector2 inputViewportPosition, bool isXInRightCurveArea, bool isYInUpCurveArea)
    {
        Vector2 standard;
        Vector2 velocity;
        float percent;
        float speed;

        float inputX = inputViewportPosition.x;
        float inputY = inputViewportPosition.y;

        if (isXInRightCurveArea)
        {
            if (isYInUpCurveArea) // 우상단
            {
                standard = new Vector2(data.MaxCurveAreaValue, data.MaxCurveAreaValue);
            }
            else // 우하단
            {
                standard = new Vector2(data.MaxCurveAreaValue, data.MinCurveAreaValue);
            }
        }
        else
        {
            if (isYInUpCurveArea) // 좌상단
            {
                standard = new Vector2(data.MinCurveAreaValue, data.MaxCurveAreaValue);
            }
            else // 좌하단
            {
                standard = new Vector2(data.MinCurveAreaValue, data.MinCurveAreaValue);
            }
        }

        float signX = Mathf.Sign(inputX);
        float signY = Mathf.Sign(inputY);

        if (signX == signY)
        {
            // 1사분면, 3사분면
            float slope = (inputY - standard.y) / (inputX - standard.x);
            float cornerSlope = (1 - standard.y) / (1 - standard.x);

            if (slope >= cornerSlope)
            {
                TryGetXAtY(standard, inputViewportPosition, 1, out Vector2 result1);
                TryGetXAtY(standard, inputViewportPosition, data.MaxMoveAreaValue, out Vector2 result2);

                float full = Vector2.SqrMagnitude(result1 - result2);
                float input = Vector2.SqrMagnitude(inputViewportPosition - result2);

                percent = input / full;
            }
            else
            {
                TryGetYAtX(standard, inputViewportPosition, 1, out Vector2 result1);
                TryGetYAtX(standard, inputViewportPosition, data.MaxMoveAreaValue, out Vector2 result2);

                float full = Vector2.SqrMagnitude(result1 - result2);
                float input = Vector2.SqrMagnitude(inputViewportPosition - result2);

                percent = input / full;
            }
        }
        else
        {
            // 2사분면, 4사분면
            float slope = (inputY - standard.y) / (inputX - standard.x);
            float cornerSlope = (1 - standard.y) / (1 - standard.x);

            if (slope >= cornerSlope)
            {
                TryGetYAtX(standard, inputViewportPosition, 1, out Vector2 result1);
                TryGetYAtX(standard, inputViewportPosition, data.MaxMoveAreaValue, out Vector2 result2);

                float full = Vector2.SqrMagnitude(result1 - result2);
                float input = Vector2.SqrMagnitude(inputViewportPosition - result2);

                percent = input / full;
            }
            else
            {
                TryGetXAtY(standard, inputViewportPosition, 1, out Vector2 result1);
                TryGetXAtY(standard, inputViewportPosition, data.MaxMoveAreaValue, out Vector2 result2);

                float full = Vector2.SqrMagnitude(result1 - result2);
                float input = Vector2.SqrMagnitude(inputViewportPosition - result2);

                percent = input / full;
            }
        }

        Vector2 direction = (inputViewportPosition - standard).normalized;
        if(percent > 1) percent = 1;
        speed = percent * data.MaxSpeed;
        velocity = speed * Time.deltaTime * direction;

        return velocity;
    }

    /// <summary>
    /// 두 점 a, b를 지나는 직선에서 y 좌표가 c일 때의 좌표를 구합니다.
    /// </summary>
    private bool TryGetXAtY(Vector2 a, Vector2 b, float c, out Vector2 result)
    {
        // 두 점의 y 좌표가 같으면 수평선이므로, y = c가 아닌 이상 x 값을 특정할 수 없습니다.
        if (Mathf.Approximately(a.y, b.y))
        {
            result = Vector2.zero;
            return false;
        }

        // 직선의 방정식 변형 공식 적용
        float resultX = a.x + (c - a.y) * (b.x - a.x) / (b.y - a.y);

        result = new Vector2(resultX, c);

        return true;
    }

    /// <summary>
    /// 두 점 a, b를 지나는 직선에서 x 좌표가 c일 때의 좌표를 구합니다.
    /// </summary>
    private bool TryGetYAtX(Vector2 a, Vector2 b, float c, out Vector2 result)
    {
        // 두 점의 y 좌표가 같으면 수평선이므로, x = c가 아닌 이상 y 값을 특정할 수 없습니다.
        if (Mathf.Approximately(a.y, b.y))
        {
            result = Vector2.zero;
            return false;
        }

        // 직선의 방정식 변형 공식 적용
        float resultY = a.y + (b.y - a.y) * (c - a.x) / (b.x - a.x);

        result = new Vector2(c, resultY);

        return true;
    }
}
