using UnityEngine;

[DefaultExecutionOrder(1000)]
public sealed class ParallaxLayer : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float followX;
    [SerializeField, Range(0f, 1f)] private float followY;
    [SerializeField] private Vector2 cameraOrigin;

    private Vector3 layerOrigin;
    private Camera gameplayCamera;

    private void Awake()
    {
        layerOrigin = transform.position;
    }

    private void LateUpdate()
    {
        if (gameplayCamera == null) gameplayCamera = Camera.main;
        if (gameplayCamera == null) return;

        Vector2 offset = (Vector2)gameplayCamera.transform.position - cameraOrigin;
        transform.position = layerOrigin + new Vector3(offset.x * followX, offset.y * followY, 0f);
    }
}
