using UnityEngine;

public static class TransformExtensions
{
    private const float DISTANCE_TO_REACH = 0.001f;

    public static void SetIntPosition(this Transform transform, Vector2Int pos) =>
        transform.position = new Vector3(pos.x, pos.y);

    #region ==== Distance Checks ====

    public static bool IsReach(this Transform target, Vector3 point) =>
        IsReach(target.position, point);

    public static bool IsReach(this Transform target, Transform point) =>
        IsReach(target.position, point.position);

    private static bool IsReach(Vector3 target, Vector3 point)
    {
        return (target - point).sqrMagnitude < DISTANCE_TO_REACH;
    }

    #endregion

    #region ==== Move ====

    public static void MoveTowards(this Transform movable, Transform target, float speed) =>
        MoveTowards(movable, target.position, speed);

    public static void MoveTowards(this Transform movable, Vector3 target, float speed) =>
        Move(movable, target, speed);

    private static void Move(Transform movable, Vector3 target, float speed)
    {
        movable.position = Vector3.MoveTowards(movable.position, target, speed * Time.deltaTime);
    }

    #endregion

    #region ==== Look / Follow ====

    public static void LookAt2D(this Transform movable, Transform target) =>
        LookAt2D(movable, target.position);

    public static void LookAt2D(this Transform movable, Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - movable.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        movable.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
    public static void RotateAt2D(this Transform movable, Transform target, float rotationSpeed = 720f) =>
        RotateAt2D(movable, target.position, rotationSpeed);

    public static void RotateAt2D(this Transform movable, Vector3 target, float rotationSpeed = 720f)
    {
        Vector3 dir = target - movable.position;
        if (dir.sqrMagnitude < 0.0001f)
            return;

        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        movable.rotation = Quaternion.RotateTowards(movable.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Move and look to target
    /// </summary>
    public static void FollowTarget(this Transform movable, Vector3 target, float moveSpeed, float rotationSpeed = 720f)
    {
        if (rotationSpeed <= 0)
            LookAt2D(movable, target);
        else
            RotateAt2D(movable, target, rotationSpeed);

        MoveTowards(movable, target, moveSpeed);
    }

    #endregion
}
