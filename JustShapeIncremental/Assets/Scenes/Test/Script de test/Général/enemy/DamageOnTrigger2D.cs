using UnityEngine;

public class DamageOnTrigger2D : MonoBehaviour
{
    public enum HitBehavior
    {
        DoNothing,
        Destroy,
        ReturnToPool
    }

    [Header("Damage Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private bool isDamageActive = true;

    [Header("Hit Behavior")]
    [SerializeField] private HitBehavior hitBehavior =
        HitBehavior.ReturnToPool;

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
        if (!isDamageActive)
            return;

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        bool damageApplied = playerHealth.TakeDamage(
            damage,
            transform.position
        );

        if (!damageApplied)
            return;

        HandleHit();
    }

    private void HandleHit()
    {
        switch (hitBehavior)
        {
            case HitBehavior.DoNothing:
                break;

            case HitBehavior.Destroy:
                Destroy(transform.root.gameObject);
                break;

            case HitBehavior.ReturnToPool:
            {
                IPoolable poolable =
                    transform.root.GetComponent<IPoolable>();

                if (poolable != null)
                {
                    poolable.ReturnToPool();
                }
                else
                {
                    Debug.LogError(
                        $"{transform.root.name} ne possède pas " +
                        "de composant implémentant IPoolable.",
                        this
                    );
                }

                break;
            }
        }
    }

    public void EnableDamage()
    {
        isDamageActive = true;
    }

    public void DisableDamage()
    {
        isDamageActive = false;
    }

    public void SetDamageActive(bool active)
    {
        isDamageActive = active;
    }
}