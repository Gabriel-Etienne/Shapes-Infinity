
using UnityEngine;

public class ZoneVisualTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private DamageOnTrigger2D damageTrigger;

    [Header("Screen Shake")]
    [SerializeField] private float shakeMagnitude = 0.15f;
    [SerializeField] private float shakeDuration = 0.2f;

    [Header("Scale Settings")]
    [SerializeField] private float startScale = 0.2f;
    [SerializeField] private float maxScale = 3f;

    [Header("Spawn Bounce")]
    [SerializeField] private float spawnBounceDuration = 0.2f;
    [SerializeField] private AnimationCurve spawnBounceCurve =
        new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.7f, 1.3f),
            new Keyframe(1f, 1f)
        );

    [Header("Growth Settings")]
    [SerializeField] private float growDuration = 1f;
    [SerializeField] private float visibleDuration = 0.5f;

    [Header("End Bounce")]
    [SerializeField] private float endBounceDuration = 0.2f;
    [SerializeField] private AnimationCurve endBounceCurve =
        new AnimationCurve(
            new Keyframe(0f, 0.8f),
            new Keyframe(0.7f, 1.1f),
            new Keyframe(1f, 1f)
        );

    [SerializeField, Range(0f, 1f)]
    private float colorChangeProgress = 0.7f;

    [Header("Visual Settings")]
    [SerializeField] private Color zoneColor = Color.red;

    [SerializeField, Range(0f, 1f)]
    private float initialAlpha = 0.2f;

    [Header("Damage Settings")]
    [SerializeField, Range(0f, 1f)]
    private float damageActivationProgress = 0.7f;

    private ZonePoolTest pool;

    private float timer;

    private bool isSpawning;
    private bool isGrowing;
    private bool isEndBouncing;
    private bool isVisible;

    private bool hasChangedColor;
    private bool hasActivatedDamage;

    private Vector3 finalScale;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (damageTrigger == null)
            damageTrigger = GetComponent<DamageOnTrigger2D>();

        // Sécurité : une zone ne doit jamais infliger
        // de dégâts avant son activation.
        damageTrigger?.DisableDamage();
    }

    private void OnEnable()
    {
        damageTrigger?.DisableDamage();
    }

    private void OnDisable()
    {
        damageTrigger?.DisableDamage();
    }

    public void Initialize(
        ZonePoolTest zonePool,
        CameraShakeTest shakeCamera)
    {
        pool = zonePool;

        timer = 0f;

        isSpawning = true;
        isGrowing = false;
        isEndBouncing = false;
        isVisible = false;

        hasChangedColor = false;
        hasActivatedDamage = false;

        // Toujours désactiver les dégâts au spawn.
        damageTrigger?.DisableDamage();

        finalScale = Vector3.one * maxScale;

        transform.localScale = Vector3.zero;

        SetAlpha(initialAlpha);
    }

    private void Update()
    {
        if (isSpawning)
        {
            UpdateSpawnBounce();
            return;
        }

        if (isGrowing)
        {
            UpdateGrowth();
            return;
        }

        if (isEndBouncing)
        {
            UpdateEndBounce();
            return;
        }

        if (isVisible)
        {
            timer += Time.deltaTime;

            if (timer >= visibleDuration)
                ReturnToPool();
        }
    }

    private void UpdateSpawnBounce()
    {
        timer += Time.deltaTime;

        float progress = spawnBounceDuration > 0f
            ? Mathf.Clamp01(timer / spawnBounceDuration)
            : 1f;

        float curveValue = spawnBounceCurve.Evaluate(progress);

        float currentScale = startScale * curveValue;

        transform.localScale = Vector3.one * currentScale;

        if (progress >= 1f)
        {
            transform.localScale = Vector3.one * startScale;

            isSpawning = false;
            isGrowing = true;

            timer = 0f;
        }
    }

    private void UpdateGrowth()
    {
        timer += Time.deltaTime;

        float progress = growDuration > 0f
            ? Mathf.Clamp01(timer / growDuration)
            : 1f;

        float currentScale = Mathf.Lerp(
            startScale,
            maxScale * 0.8f,
            progress
        );

        transform.localScale = Vector3.one * currentScale;

        if (progress >= 1f)
        {
            isGrowing = false;
            isEndBouncing = true;

            timer = 0f;
        }
    }

    private void UpdateEndBounce()
    {
        timer += Time.deltaTime;

        float progress = endBounceDuration > 0f
            ? Mathf.Clamp01(timer / endBounceDuration)
            : 1f;

        float curveValue = endBounceCurve.Evaluate(progress);

        float currentScale = maxScale * curveValue;

        transform.localScale = Vector3.one * currentScale;

        // Changement de couleur.
        if (!hasChangedColor && progress >= colorChangeProgress)
        {
            SetAlpha(1f);

            CameraShakeTest.TriggerShake?.Invoke(
                shakeDuration,
                shakeMagnitude
            );

            hasChangedColor = true;
        }

        // Activation des dégâts.
        if (!hasActivatedDamage &&
            progress >= damageActivationProgress)
        {
            damageTrigger?.EnableDamage();

            hasActivatedDamage = true;

            Debug.Log("Zone damage activated!");
        }

        if (progress >= 1f)
        {
            transform.localScale = finalScale;

            SetAlpha(1f);

            isEndBouncing = false;
            isVisible = true;

            timer = 0f;
        }
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer == null)
            return;

        Color color = zoneColor;
        color.a = alpha;

        spriteRenderer.color = color;
    }

    private void ReturnToPool()
    {
        isVisible = false;

        // Sécurité avant de remettre la zone dans le pool.
        damageTrigger?.DisableDamage();

        if (pool != null)
        {
            pool.ReturnZone(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
