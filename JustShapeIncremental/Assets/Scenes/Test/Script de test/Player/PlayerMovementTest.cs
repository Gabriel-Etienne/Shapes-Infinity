using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementTest : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private PlayerInputTest playerInput;
    private PlayerStateTest playerState;
    private Rigidbody2D rb;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInputTest>();
        playerState = GetComponent<PlayerStateTest>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Ne pas modifier la vitesse pendant un dash,
        // un knockback ou lorsque le joueur est mort.
        if (!playerState.CanChangeVelocity ||
            playerState.IsDashing ||
            playerState.IsDead)
        {
            return;
        }

        Vector2 movement = playerInput.Movement.normalized;

        rb.linearVelocity = movement * moveSpeed;
    }
}