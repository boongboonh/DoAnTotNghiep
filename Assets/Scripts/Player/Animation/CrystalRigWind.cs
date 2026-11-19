using UnityEngine;

/// <summary>Draws the Crystal tail and adds wind motion to its ears.</summary>
[DefaultExecutionOrder(80)]
public sealed class CrystalRigWind : MonoBehaviour
{
    private const int SamplesPerSpan = 10;
    private readonly Vector3[] points = new Vector3[5];
    private readonly Vector3[] speeds = new Vector3[5];
    private readonly Vector3[] curve = new Vector3[4 * SamplesPerSpan + 1];
    private Rigidbody2D body;
    private SpriteRenderer tail;
    private Transform root;
    private LineRenderer ribbon;
    private Transform[] ears;
    private Quaternion[] earRest;
    private float[] earAngles;
    private float[] earSpeeds;
    private Vector3 lastRoot;
    private float length;
    private bool initialized;

    internal void Bind(SpriteRenderer source, Transform hip, Transform leftEar, Transform rightEar)
    {
        tail = source;
        root = hip;
        body = GetComponentInParent<Rigidbody2D>();
        ears = new[] { leftEar, rightEar };
        earRest = new[] { leftEar.localRotation, rightEar.localRotation };
        earAngles = new float[2];
        earSpeeds = new float[2];
        length = Mathf.Max(0.1f, tail.bounds.size.x);

        var visual = new GameObject("Crystal tail ribbon");
        visual.transform.SetParent(transform, false);
        ribbon = visual.AddComponent<LineRenderer>();
        ribbon.useWorldSpace = true;
        ribbon.sharedMaterial = tail.sharedMaterial;
        ribbon.sortingLayerID = tail.sortingLayerID;
        ribbon.sortingOrder = tail.sortingOrder;
        ribbon.positionCount = curve.Length;
        ribbon.numCornerVertices = 2;
        ribbon.numCapVertices = 2;
        ribbon.widthMultiplier = length * 0.055f;
        ribbon.widthCurve = new AnimationCurve(
            new Keyframe(0f, 0.65f), new Keyframe(0.32f, 1f), new Keyframe(1f, 0.03f));
        ribbon.startColor = new Color(0.88f, 0.95f, 1f);
        ribbon.endColor = new Color(0.55f, 0.64f, 1f, 0.75f);
        tail.enabled = false;
    }

    private void OnEnable()
    {
        initialized = false;
        if (ribbon == null) return;
        ribbon.enabled = true;
        tail.enabled = false;
    }

    private void OnDisable()
    {
        if (ribbon == null) return;
        ribbon.enabled = false;
        tail.enabled = true;
        for (int i = 0; i < ears.Length; i++)
        {
            ears[i].localRotation = earRest[i];
            earAngles[i] = earSpeeds[i] = 0f;
        }
    }

    private void LateUpdate()
    {
        if (ribbon == null) return;
        Vector3 origin = root.position;
        bool reset = !initialized || (origin - lastRoot).sqrMagnitude > 4f;
        initialized = true;
        lastRoot = origin;
        Vector2 velocity = body != null ? body.linearVelocity : Vector2.zero;
        float facing = transform.right.x < 0f ? -1f : 1f;
        float drag = Mathf.Clamp(velocity.x / 7f, -2f, 2f);
        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        int steps = Mathf.Max(1, Mathf.CeilToInt(dt * 120f));
        float step = dt / steps;
        points[0] = origin;

        for (int i = 1; i < points.Length; i++)
        {
            float t = i / 4f;
            float x = -facing * Mathf.Sin(t * 2.4f) - drag * t * 0.43f;
            float y = -Mathf.Sin(t * 3.7f) * 0.47f
                - Mathf.Clamp(velocity.y / 12f, -1f, 1f) * t * 0.3f;
            Vector3 target = origin + new Vector3(x, y, 0f) * length;
            if (reset)
            {
                points[i] = target;
                speeds[i] = Vector3.zero;
            }
            else
            {
                float stiffness = Mathf.Lerp(175f, 72f, t);
                for (int j = 0; j < steps; j++)
                {
                    speeds[i] += ((target - points[i]) * stiffness - speeds[i] * 15f) * step;
                    points[i] += speeds[i] * step;
                }
                points[i] = origin + Vector3.ClampMagnitude(points[i] - origin, length * 1.85f);
            }
        }

        float earTarget = Mathf.Clamp(drag * facing * 11f - velocity.y * 0.8f, -24f, 24f);
        for (int i = 0; i < ears.Length; i++)
        {
            float stiffness = i == 0 ? 145f : 105f;
            for (int j = 0; j < steps; j++)
            {
                earSpeeds[i] += ((earTarget - earAngles[i]) * stiffness - earSpeeds[i] * 13f) * step;
                earAngles[i] += earSpeeds[i] * step;
            }
            ears[i].localRotation = earRest[i] * Quaternion.Euler(0f, 0f, earAngles[i]);
        }

        for (int span = 0; span < points.Length - 1; span++)
        {
            Vector3 a = points[Mathf.Max(0, span - 1)];
            Vector3 b = points[span];
            Vector3 c = points[span + 1];
            Vector3 d = points[Mathf.Min(points.Length - 1, span + 2)];
            for (int i = 0; i < SamplesPerSpan; i++)
            {
                float t = i / (float)SamplesPerSpan;
                curve[span * SamplesPerSpan + i] = 0.5f * (2f * b + (c - a) * t
                    + (2f * a - 5f * b + 4f * c - d) * t * t
                    + (-a + 3f * b - 3f * c + d) * t * t * t);
            }
        }
        curve[curve.Length - 1] = points[points.Length - 1];
        ribbon.SetPositions(curve);
    }
}
