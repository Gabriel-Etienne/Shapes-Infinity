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
        if (playerState != null && playerState.IsDashing)
            return;

        Vector2 movement = playerInput.Movement;

        rb.linearVelocity = movement.normalized * moveSpeed;
    }
}