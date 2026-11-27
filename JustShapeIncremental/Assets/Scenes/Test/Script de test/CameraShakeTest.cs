using System;
using System.Collections;
using UnityEngine;

public class CameraShakeTest : MonoBehaviour
{
    [Header("Shake Settings")] [SerializeField]
    private float defaultDuration = 0.2f;

    [SerializeField] private float defaultMagnitude = 0.15f;

    // Events statiques
    public static Action<float, float> TriggerShake;
    public static Action TriggerDefaultShake;

    private Vector3 initialPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        initialPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        TriggerShake += Shake;
        TriggerDefaultShake += ShakeDefault;
    }

    private void OnDisable()
    {
        TriggerShake -= Shake;
        TriggerDefaultShake -= ShakeDefault;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        transform.localPosition = initialPosition;
    }

    private void ShakeDefault()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    private void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(
            ShakeCoroutine(duration, magnitude)
        );
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float timer = 0f;

        while (timer < duration)
        {
            float remainingTime = 1f - (timer / duration);

            Vector2 randomOffset =
                magnitude * remainingTime * UnityEngine.Random.insideUnitCircle;

            transform.localPosition = initialPosition +
                                      new Vector3(randomOffset.x, randomOffset.y, 0f);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = initialPosition;
        shakeCoroutine = null;
    }
}