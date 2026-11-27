using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageOnTrigger2D : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private bool isDamageActive = true;

    [Header("Collision Settings")]
    [SerializeField] private bool destroyOnHit = true;

    private bool hasHit;

    public bool IsDamageActive => isDamageActive;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (!isDamageActive || hasHit)
            return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        bool damageApplied = playerHealth.TakeDamage(
            damage,
            transform.position
        );

        if (!damageApplied)
            return;

        hasHit = true;

        Debug.Log($"Damage zone hit player for {damage} damage!");

        if (destroyOnHit)
            Destroy(gameObject);
    }

    public void EnableDamage()
    {
        isDamageActive = true;
        hasHit = false;
    }

    public void DisableDamage()
    {
        isDamageActive = false;
    }

    public void SetDamageActive(bool active)
    {
        isDamageActive = active;

        if (active)
            hasHit = false;
    }
}