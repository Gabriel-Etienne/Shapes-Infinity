using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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
        // Verrouiller la rotation pendant le Dash
        if (playerState != null && playerState.IsDashing)
            return;

        Vector2 direction = rb.linearVelocity;

        // Ne pas tourner si le joueur ne bouge pas
        if (direction.sqrMagnitude < 0.05f * 0.05f)
            return;

        // Calculer l'angle de déplacement
        float targetAngle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        // Rotation progressive
        float newAngle = Mathf.MoveTowardsAngle(
            rb.rotation,
            targetAngle,
            normalRotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newAngle);
    }
}