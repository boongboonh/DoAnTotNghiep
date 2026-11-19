using UnityEngine;

/// <summary>
/// Camera prototype (CAM-01): nhin lech theo huong di, vung dem doc (len thoai mai, xuong chat de thay san dap),
/// gioi han theo man. Thuan tinh toan vi tri; Playground gan vao camera.
/// </summary>
public sealed class PrototypeCameraFollow
{
    public float lookAhead = 2.2f;
    public float upDeadZone = 1.6f;
    public float downDeadZone = .9f;
    public float verticalOffset = .5f;
    public float smoothTime = .18f;
    public float lookSmoothTime = .3f;
    public Vector2 minBounds = new Vector2(-16f, 2f);
    public Vector2 maxBounds = new Vector2(91f, 8f);

    private Vector3 velocity;
    private float lookX;
    private float lookVelocity;
    private float anchorY;

    /// <summary>Dat lai khong con smoothing cu; tra vi tri camera ngay tai nguoi choi.</summary>
    public Vector3 Snap(Vector2 player, int facing)
    {
        velocity = Vector3.zero;
        lookVelocity = 0f;
        lookX = facing >= 0 ? lookAhead : -lookAhead;
        anchorY = player.y;
        return Desired(player);
    }

    public Vector3 Step(Vector3 current, Vector2 player, int facing, float dt)
    {
        lookX = Mathf.SmoothDamp(lookX, facing >= 0 ? lookAhead : -lookAhead, ref lookVelocity, lookSmoothTime,
            Mathf.Infinity, dt);
        if (player.y > anchorY + upDeadZone) anchorY = player.y - upDeadZone;
        else if (player.y < anchorY - downDeadZone) anchorY = player.y + downDeadZone;
        return Vector3.SmoothDamp(current, Desired(player), ref velocity, smoothTime, Mathf.Infinity, dt);
    }

    private Vector3 Desired(Vector2 player)
    {
        float x = Mathf.Clamp(player.x + lookX, minBounds.x, maxBounds.x);
        float y = Mathf.Clamp(anchorY + verticalOffset, minBounds.y, maxBounds.y);
        return new Vector3(x, y, -10f);
    }
}
