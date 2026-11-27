using System.Collections;
using UnityEngine;

public class PlayerDeathSlowMotion : MonoBehaviour
{
    [Header("Slow Motion Settings")]
    [SerializeField] private float slowdownDuration = 1.5f;
    [SerializeField] private float targetTimeScale = 0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource slowdownAudio;
    [SerializeField] private float targetPitch = 0.2f;

    private bool hasStarted;
    private Coroutine slowdownCoroutine;

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        if (hasStarted)
            return;

        hasStarted = true;
        slowdownCoroutine = StartCoroutine(SlowdownCoroutine());
    }

    private IEnumerator SlowdownCoroutine()
    {
        float startTimeScale = Time.timeScale;
        float startPitch = slowdownAudio != null
            ? slowdownAudio.pitch
            : 1f;

        if (slowdownAudio != null)
            slowdownAudio.Play();

        float elapsed = 0f;

        while (elapsed < slowdownDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / slowdownDuration);

            // Ralentissement du temps.
            Time.timeScale = Mathf.Lerp(
                startTimeScale,
                targetTimeScale,
                t
            );

            //Ralentissement du son.
            if (slowdownAudio != null)
            {
                slowdownAudio.pitch = Mathf.Lerp(
                    startPitch,
                    targetPitch,
                    t
                );
            }

            yield return null;
        }

        Time.timeScale = targetTimeScale;

        if (slowdownAudio != null)
            slowdownAudio.pitch = targetPitch;

        slowdownCoroutine = null;
    }
}