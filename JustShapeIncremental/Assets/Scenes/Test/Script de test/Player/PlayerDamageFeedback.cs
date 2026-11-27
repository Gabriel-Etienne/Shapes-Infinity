using UnityEngine;

public class PlayerDamageFeedback : MonoBehaviour
{
    [Header("Screen Shake")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 0.2f;

    public void PlayDamageFeedback()
    {
        CameraShakeTest.TriggerShake?.Invoke(
            shakeDuration,
            shakeMagnitude
        );
    }
}