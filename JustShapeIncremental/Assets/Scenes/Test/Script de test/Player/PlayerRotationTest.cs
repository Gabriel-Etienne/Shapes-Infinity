using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStateTest))]
public class PlayerRotationTest : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float normalRotationSpeed = 720f;

    private Rigidbody2D rb;
    private PlayerStateTest playerState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerStateTest>();
    }

    private void FixedUpdate()
    {
        // Ne pas tourner pendant le dash, le knockback ou la mort.
        if (playerState.IsDashing ||
            !playerState.CanChangeVelocity ||
            playerState.IsDead)
        {
            return;
        }

        Vector2 velocity = rb.linearVelocity;

        // Évite les rotations lorsque le joueur est presque immobile.
        if (velocity.sqrMagnitude < 0.0025f)
            return;

        float targetAngle =
            Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

        float currentAngle = rb.rotation;

        float newAngle = Mathf.MoveTowardsAngle(
            currentAngle,
            targetAngle,
            normalRotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newAngle);
    }
}