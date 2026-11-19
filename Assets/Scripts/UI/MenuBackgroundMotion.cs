using UnityEngine;

[DisallowMultipleComponent]
public sealed class MenuBackgroundMotion : MonoBehaviour
{
    [SerializeField] private Camera menuCamera;
    [SerializeField] private SpriteRenderer landscape;
    [SerializeField] private SpriteRenderer mist;
    [SerializeField] private SpriteRenderer foreground;
    [SerializeField] private bool gentleBlossomMotion;

    private static readonly int MotionTime = Shader.PropertyToID("_MotionTime");
    private MaterialPropertyBlock windProperties;
    private float elapsed;
    private Color baseMistColor;

    private void OnEnable()
    {
        elapsed = 0f;
        if (mist) baseMistColor = mist.color;
        PositionLayers();
    }

    private void OnDisable()
    {
        if (gentleBlossomMotion && mist) mist.color = baseMistColor;
    }

    private void LateUpdate()
    {
        elapsed += Time.unscaledDeltaTime;
        PositionLayers();
    }

    private void PositionLayers()
    {
        if (!menuCamera || !menuCamera.orthographic) return;

        float height = menuCamera.orthographicSize * 2f;
        Vector2 viewport = new Vector2(height * menuCamera.aspect, height);
        Vector2 center = menuCamera.transform.position;

        if (gentleBlossomMotion)
        {
            Place(landscape, center, viewport, Vector2.zero, 1f);
            Place(mist, center, viewport, new Vector2(0.009f, 0.003f), 38f);
            UpdateWind(landscape);
            UpdateWind(mist);
            if (mist)
            {
                Color tint = baseMistColor;
                tint.a *= 0.93f + 0.07f * Mathf.Cos(elapsed * (2f * Mathf.PI / 26f));
                mist.color = tint;
            }
            return;
        }

        Place(landscape, center, viewport, new Vector2(0.005f, 0.004f), 36f);
        Place(mist, center, viewport, new Vector2(0.030f, 0.008f), 23f);
        Place(foreground, center, viewport, new Vector2(0.014f, 0.006f), 15f);
    }

    private void UpdateWind(SpriteRenderer layer)
    {
        if (!layer) return;

        windProperties ??= new MaterialPropertyBlock();
        layer.GetPropertyBlock(windProperties);
        windProperties.SetFloat(MotionTime, elapsed);
        layer.SetPropertyBlock(windProperties);
    }

    private void Place(SpriteRenderer layer, Vector2 center, Vector2 viewport, Vector2 travel, float period)
    {
        if (!layer || !layer.sprite) return;

        Vector2 size = layer.sprite.bounds.size;
        Vector2 padding = Vector2.Scale(viewport, travel);
        Vector2 covered = viewport + padding * 2f;
        float scale = Mathf.Max(covered.x / size.x, covered.y / size.y);
        float phase = elapsed * (2f * Mathf.PI / period);
        Vector2 offset = Vector2.Scale(padding, new Vector2(Mathf.Sin(phase), Mathf.Sin(phase * 2f)));

        layer.transform.localScale = new Vector3(scale, scale, 1f);
        layer.transform.position = new Vector3(center.x + offset.x, center.y + offset.y, layer.transform.position.z);
    }
}
