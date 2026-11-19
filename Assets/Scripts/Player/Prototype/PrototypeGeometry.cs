using UnityEngine;

/// <summary>Hinh khoi va nhan trong world, khong phu thuoc Canvas hoac art moi.</summary>
public static class PrototypeGeometry
{
    public static SpriteRenderer Rectangle(Transform parent, Sprite sprite, string name,
        Vector2 size, Vector2 position, Color color, int order = 5)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = position;
        g.transform.localScale = new Vector3(size.x, size.y, 1f);
        var renderer = g.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = order;
        return renderer;
    }

    public static TextMesh Label(Transform parent, string name, Vector2 position, float size)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = position;
        var label = g.AddComponent<TextMesh>();
        label.anchor = TextAnchor.MiddleCenter;
        label.characterSize = size;
        label.fontSize = 48;
        return label;
    }
}
