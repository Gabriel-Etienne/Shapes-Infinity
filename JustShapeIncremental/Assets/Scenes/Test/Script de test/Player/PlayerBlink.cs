
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInvulnerability))]
public class PlayerBlink : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float blinkInterval = 0.1f;

    [SerializeField, Range(0f, 1f)]
    private float blinkAlpha = 0.2f;

    private PlayerInvulnerability invulnerability;

    private Coroutine blinkCoroutine;
    private Color originalSpriteColor;

    private void Awake()
    {
        invulnerability = GetComponent<PlayerInvulnerability>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalSpriteColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        if (invulnerability == null)
            return;

        invulnerability.OnInvulnerabilityStarted += StartBlink;
        invulnerability.OnInvulnerabilityEnded += StopBlink;
    }

    private void OnDisable()
    {
        if (invulnerability != null)
        {
            invulnerability.OnInvulnerabilityStarted -= StartBlink;
            invulnerability.OnInvulnerabilityEnded -= StopBlink;
        }

        StopBlink();
    }

    private void StartBlink()
    {
        StopBlink();

        if (spriteRenderer == null)
            return;

        blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        bool visible = true;

        while (true)
        {
            Color color = originalSpriteColor;

            color.a = visible
                ? originalSpriteColor.a
                : blinkAlpha;

            spriteRenderer.color = color;

            visible = !visible;

            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalSpriteColor;
    }
}
