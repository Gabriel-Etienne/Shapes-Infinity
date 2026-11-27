using UnityEngine;

public class PlayerDamageFeedback : MonoBehaviour
{
    [Header("Screen Shake")]
    [SerializeField] private SFXType playerHitSFX = SFXType.PlayerHit;
    [SerializeField] private float hitShakeDuration = 0.2f;
    [SerializeField] private float hitShakeMagnitude = 0.2f;
    
    [Header("Screen Shake")]
    [SerializeField] private SFXType playerDeathSFX = SFXType.PlayerDeath;
    [SerializeField] private float deathShakeDuration = 0.2f;
    [SerializeField] private float deathShakeMagnitude = 0.2f;
    
    private void OnEnable()
    {
        PlayerHealth.OnPlayerHit += PlayDamageFeedback;
        PlayerHealth.OnPlayerDeath += PlayDeathFeedback;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerHit -= PlayDamageFeedback;
        PlayerHealth.OnPlayerDeath -= PlayDeathFeedback;
    }
    
    public void PlayDamageFeedback()
    {
        CameraShakeTest.TriggerShake?.Invoke(
            hitShakeDuration,
            hitShakeMagnitude
        );
        
        SFXManager.Instance?.Play(playerHitSFX);
    }
    
    public void PlayDeathFeedback()
    {
        CameraShakeTest.TriggerShake?.Invoke(
            deathShakeDuration,
            deathShakeMagnitude
        );
        
        SFXManager.Instance?.Play(playerDeathSFX);
    }
}