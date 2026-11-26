using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDashTest : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    private PlayerInputTest playerInput;
    private PlayerStateTest playerState;
    private Rigidbody2D rb;

    private Vector2 lastMovementDirection = Vector2.right;

    private float dashTimer;
    private float cooldownTimer;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInputTest>();
        playerState = GetComponent<PlayerStateTest>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (playerInput != null)
            playerInput.OnDashPerformed += Dash;
    }

    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.OnDashPerformed -= Dash;

        if (playerState != null)
            playerState.IsDashing = false;
    }

    private void Update()
    {
        Vector2 movement = playerInput.Movement;

        if (movement != Vector2.zero)
            lastMovementDirection = movement.normalized;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!playerState.IsDashing)
            return;

        dashTimer -= Time.fixedDeltaTime;

        if (dashTimer <= 0f)
            playerState.IsDashing = false;
    }

    private void Dash()
    {
        if (playerState.IsDashing || cooldownTimer > 0f)
            return;

        playerState.IsDashing = true;

        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;

        rb.linearVelocity = lastMovementDirection * dashForce;
    }
}