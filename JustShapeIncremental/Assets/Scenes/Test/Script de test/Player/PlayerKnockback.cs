
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStateTest))]
public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private PlayerStateTest playerState;

    private Coroutine knockbackCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerStateTest>();
    }

    public void Apply(Vector2 damageSourcePosition)
    {
        if (playerState.IsDead)
            return;

        Vector2 direction =
            (Vector2)transform.position - damageSourcePosition;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.up;

        direction.Normalize();

        // Interrompt un éventuel knockback précédent.
        StopCurrentKnockback();

        playerState.SetCanChangeVelocity(false);

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(
            direction * knockbackForce,
            ForceMode2D.Impulse
        );

        knockbackCoroutine = StartCoroutine(KnockbackCoroutine());
    }

    private IEnumerator KnockbackCoroutine()
    {
        yield return new WaitForSeconds(knockbackDuration);

        knockbackCoroutine = null;

        if (!playerState.IsDead)
            playerState.SetCanChangeVelocity(true);
    }

    public void Cancel()
    {
        StopCurrentKnockback();

        rb.linearVelocity = Vector2.zero;
    }

    private void StopCurrentKnockback()
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }
    }

    private void OnDisable()
    {
        StopCurrentKnockback();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (playerState != null && !playerState.IsDead)
            playerState.SetCanChangeVelocity(true);
    }
}
