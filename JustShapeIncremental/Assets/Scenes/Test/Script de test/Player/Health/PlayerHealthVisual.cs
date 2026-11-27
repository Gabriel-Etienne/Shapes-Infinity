using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHealthVisual : MonoBehaviour
{
    private static readonly int FillAmountID =
        Shader.PropertyToID("_FillAmount");

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.25f;
    [SerializeField] private Ease animationEase = Ease.OutQuad;

    private MaterialPropertyBlock propertyBlock;
    private Tween healthTween;

    private float currentFillAmount = 1f;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        propertyBlock = new MaterialPropertyBlock();

        SetFillAmount(1f);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0f)
            return;

        float targetFill = Mathf.Clamp01(currentHealth / maxHealth);

        healthTween?.Kill();

        healthTween = DOTween.To(
            () => currentFillAmount,
            SetFillAmount,
            targetFill,
            animationDuration
        ).SetEase(animationEase);
    }

    private void SetFillAmount(float amount)
    {
        currentFillAmount = amount;

        spriteRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetFloat(FillAmountID, amount);

        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnDestroy()
    {
        healthTween?.Kill();
    }
}