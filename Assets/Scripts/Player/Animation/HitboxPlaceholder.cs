using System.Collections.Generic;
using UnityEngine;

/// <summary>Displays the main collision bounds while gameplay is being developed.</summary>
[DefaultExecutionOrder(1000)]
public sealed class HitboxPlaceholder : MonoBehaviour
{
    [SerializeField] private Color color = new Color(0.2f, 0.8f, 1f, 1f);

    private readonly Dictionary<Renderer, bool> hidden = new Dictionary<Renderer, bool>();
    private readonly List<Renderer> renderers = new List<Renderer>();
    private Collider2D hitbox;
    private SpriteRenderer shape;
    private static Sprite square;

    private void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        if (hitbox == null)
        {
            enabled = false;
            return;
        }

        if (square == null)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "Hitbox placeholder texture";
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            square = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
            square.name = "Hitbox placeholder square";
        }

        var visual = new GameObject("Hitbox placeholder");
        visual.transform.SetParent(transform, false);
        shape = visual.AddComponent<SpriteRenderer>();
        shape.sprite = square;
        shape.sortingOrder = 50;
        shape.color = color;
    }

    private void LateUpdate()
    {
        if (shape == null || hitbox == null) return;

        GetComponentsInChildren(true, renderers);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == shape || renderer.GetComponentInParent<HitboxPlaceholder>() != this) continue;
            if (!hidden.ContainsKey(renderer)) hidden.Add(renderer, renderer.forceRenderingOff);
            renderer.forceRenderingOff = true;
        }

        Bounds bounds = hitbox.bounds;
        shape.enabled = hitbox.enabled && bounds.size.sqrMagnitude > 0f;
        shape.transform.position = bounds.center;
        shape.transform.rotation = Quaternion.identity;
        Vector3 parentScale = transform.lossyScale;
        shape.transform.localScale = new Vector3(
            bounds.size.x / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)),
            bounds.size.y / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)), 1f);
    }

    private void OnEnable()
    {
        if (shape != null) shape.enabled = true;
    }

    private void OnDisable()
    {
        if (shape != null) shape.enabled = false;
        foreach (var entry in hidden)
            if (entry.Key != null) entry.Key.forceRenderingOff = entry.Value;
        hidden.Clear();
    }
}
