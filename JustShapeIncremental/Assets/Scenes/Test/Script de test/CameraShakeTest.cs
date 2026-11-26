using System.Collections;
using UnityEngine;

public class CameraShakeTest : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float defaultDuration = 0.2f;
    [SerializeField] private float defaultMagnitude = 0.15f;

    private Vector3 initialPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        initialPosition = transform.localPosition;
    }

    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

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

            Vector2 randomOffset = magnitude * remainingTime * Random.insideUnitCircle;

            transform.localPosition = initialPosition +
                                      new Vector3(randomOffset.x, randomOffset.y, 0f);

            timer += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = initialPosition;

        shakeCoroutine = null;
    }

    private void OnDisable()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        transform.localPosition = initialPosition;
    }
}