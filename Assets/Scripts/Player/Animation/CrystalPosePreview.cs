using UnityEngine;

/// <summary>Playable movement and pose inspection for the Crystal preview scene.</summary>
[RequireComponent(typeof(PosePlayer))]
public sealed class CrystalPosePreview : MonoBehaviour
{
    private PosePlayer posePlayer;
    private Camera previewCamera;
    private Vector3 spawn;
    private Vector3 cameraOffset;
    private Vector2 velocity;
    private float facing = 1f;
    private float dashTime;
    private float dashCooldown;
    private bool grounded = true;
    private bool inspecting;

    private void Awake()
    {
        posePlayer = GetComponent<PosePlayer>();
        spawn = transform.position;
        previewCamera = Camera.main;
        if (previewCamera != null) cameraOffset = previewCamera.transform.position - spawn;
    }

    private void Update()
    {
        float move = Input.GetAxisRaw("Horizontal");
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool dash = Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.LeftShift);
        if (Input.GetKeyDown(KeyCode.Backspace)) ResetPreview();
        if (Input.GetKeyDown(KeyCode.Alpha1)) Inspect("Idle");
        if (Input.GetKeyDown(KeyCode.Alpha2)) Inspect("Run");
        if (Input.GetKeyDown(KeyCode.Alpha3)) Inspect("Jump");
        if (Input.GetKeyDown(KeyCode.Alpha4)) Inspect("Fall");
        if (Input.GetKeyDown(KeyCode.Alpha5)) Inspect("Dash");
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Inspect("Idle");
            posePlayer.Push("Land", 0.18f);
        }
        if (Input.GetKeyDown(KeyCode.Tab)) Face(-facing);
        if (inspecting && move == 0f && !jump && !dash) return;
        inspecting = false;
        Advance(move, jump, Input.GetKey(KeyCode.Space), dash, Time.deltaTime);
    }

    private void Advance(float move, bool jump, bool jumpHeld, bool dash, float dt)
    {
        dashCooldown = Mathf.Max(0f, dashCooldown - dt);
        if (dashTime <= 0f && move != 0f) Face(Mathf.Sign(move));
        if (dash && dashCooldown <= 0f)
        {
            posePlayer.ClearPush();
            dashTime = 0.18f;
            dashCooldown = 0.5f;
            velocity = new Vector2(facing * 9f, 0f);
        }

        bool dashing = dashTime > 0f;
        if (dashing)
            dashTime -= dt;
        else
        {
            velocity.x = Mathf.MoveTowards(velocity.x, move * 4f, 28f * dt);
            if (jump && grounded)
            {
                posePlayer.ClearPush();
                velocity.y = 6f;
                grounded = false;
            }
            if (!grounded)
                velocity.y -= (velocity.y > 0f && jumpHeld ? 18f : 30f) * dt;
        }

        Vector3 next = transform.position + (Vector3)(velocity * dt);
        if (next.y <= spawn.y && velocity.y <= 0f)
        {
            if (!grounded) posePlayer.Push("Land", 0.12f);
            grounded = true;
            velocity.y = 0f;
            next.y = spawn.y;
        }
        transform.position = next;

        string pose = dashing ? "Dash" : !grounded ? (velocity.y > 0f ? "Jump" : "Fall")
            : Mathf.Abs(velocity.x) > 0.1f ? "Run" : "Idle";
        posePlayer.SetPose(pose);
        posePlayer.CycleSpeed = pose == "Run" ? Mathf.Clamp(Mathf.Abs(velocity.x) / 4f, 0.45f, 1f) : 1f;
    }

    private void LateUpdate()
    {
        if (previewCamera != null)
            previewCamera.transform.position = spawn + cameraOffset + Vector3.right * (transform.position.x - spawn.x);
    }

    private void Inspect(string pose)
    {
        ResetPreview();
        inspecting = true;
        posePlayer.SetPose(pose);
    }

    private void ResetPreview()
    {
        transform.position = spawn;
        velocity = Vector2.zero;
        grounded = true;
        inspecting = false;
        dashTime = dashCooldown = 0f;
        posePlayer.ClearPush();
        posePlayer.CycleSpeed = 1f;
    }

    private void Face(float direction)
    {
        facing = direction;
        transform.rotation = Quaternion.Euler(0f, direction < 0f ? 180f : 0f, 0f);
    }
}
