
using UnityEngine;

public class PlayerCameraBounds : MonoBehaviour
{
    [Header("References (Optional)")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D playerCollider;

    [Header("Bounds Settings")]
    [SerializeField] private Vector2 offset = Vector2.zero;

    private Camera targetCamera;

    private void Awake()
    {
        if (rb == null)
        {
            if (!TryGetComponent(out rb))
            {
                Debug.LogError(
                    $"No Rigidbody referenced or found on {gameObject.name}.",
                    this
                );
                enabled = false;
                return;
            }
        }

        if (playerCollider == null)
        {
            if (!TryGetComponent(out playerCollider))
            {
                Debug.LogError(
                    $"No Collider referenced or found on {gameObject.name}.",
                    this
                );
                enabled = false;
                return;
            }
        }

        targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogError("No Main Camera found.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        ClampPlayerPosition();
    }

    private void ClampPlayerPosition()
    {
        float distance = Mathf.Abs(
            transform.position.z - targetCamera.transform.position.z
        );

        Vector3 bottomLeft = targetCamera.ViewportToWorldPoint(
            new Vector3(0f, 0f, distance)
        );

        Vector3 topRight = targetCamera.ViewportToWorldPoint(
            new Vector3(1f, 1f, distance)
        );

        Vector2 extents = playerCollider.bounds.extents;

        float minX = bottomLeft.x + extents.x + offset.x;
        float maxX = topRight.x - extents.x - offset.x;

        float minY = bottomLeft.y + extents.y + offset.y;
        float maxY = topRight.y - extents.y - offset.y;

        Vector2 position = rb.position;

        Vector2 clampedPosition = new Vector2(
            Mathf.Clamp(position.x, minX, maxX),
            Mathf.Clamp(position.y, minY, maxY)
        );

        Vector2 velocity = rb.linearVelocity;

        if (!Mathf.Approximately(position.x, clampedPosition.x))
            velocity.x = 0f;

        if (!Mathf.Approximately(position.y, clampedPosition.y))
            velocity.y = 0f;

        rb.linearVelocity = velocity;
        rb.position = clampedPosition;
    }
}
