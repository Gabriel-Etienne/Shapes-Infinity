
using UnityEngine;

public class PlayerSquashStretchTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Movement Squash")]
    [SerializeField] private float stretchX = 1.08f;
    [SerializeField] private float squashY = 0.92f;

    [Header("Dash Squash")]
    [SerializeField] private float dashStretchX = 1.20f;
    [SerializeField] private float dashSquashY = 0.75f;

    [Header("Scale Transition")]
    [SerializeField] private float scaleSmoothness = 12f;

    [Header("Stop Spring")]
    [SerializeField] private float springStrength = 0.12f;
    [SerializeField] private float springFrequency = 18f;
    [SerializeField] private float springDamping = 8f;

    [Header("Movement Detection")]
    [SerializeField] private float movementThreshold = 0.05f;

    private PlayerStateTest playerState;

    private Vector3 baseScale;
    private Vector3 scaleVelocity;

    private bool wasMoving;
    private float springTimer;

    private void Awake()
    {
        baseScale = transform.localScale;

        playerState = GetComponentInParent<PlayerStateTest>();

        if (rb == null)
            rb = GetComponentInParent<Rigidbody2D>();
    }

    private void Update()
    {
        bool isMoving = rb.linearVelocity.sqrMagnitude >
                        movementThreshold * movementThreshold;

        bool isDashing = playerState != null &&
                         playerState.IsDashing;

        if (isMoving)
        {
            Vector3 targetScale;

            if (isDashing)
            {
                // Déformation plus importante pendant le Dash
                targetScale = new Vector3(
                    baseScale.x * dashStretchX,
                    baseScale.y * dashSquashY,
                    baseScale.z
                );
            }
            else
            {
                // Déformation normale pendant le déplacement
                targetScale = new Vector3(
                    baseScale.x * stretchX,
                    baseScale.y * squashY,
                    baseScale.z
                );
            }

            transform.localScale = Vector3.SmoothDamp(
                transform.localScale,
                targetScale,
                ref scaleVelocity,
                1f / scaleSmoothness
            );

            // Réinitialiser le ressort
            springTimer = 0f;
        }
        else
        {
            // Déclencher le ressort à l'arrêt
            if (wasMoving)
            {
                springTimer = 0f;
                scaleVelocity = Vector3.zero;
            }

            ApplySpring();
        }

        wasMoving = isMoving;
    }

    private void ApplySpring()
    {
        springTimer += Time.deltaTime;

        float oscillation =
            springStrength *
            Mathf.Exp(-springDamping * springTimer) *
            Mathf.Cos(springFrequency * springTimer);

        float scaleMultiplier = 1f + oscillation;

        transform.localScale = baseScale * scaleMultiplier;
    }
}
