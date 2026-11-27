using UnityEngine;

public class PlayerStateTest : MonoBehaviour
{
    public bool IsDashing { get; set; }
    public bool IsDead { get; private set; }

    // Indique si le joueur peut modifier sa vitesse normalement.
    public bool CanChangeVelocity { get; private set; } = true;

    public void SetCanChangeVelocity(bool canChange)
    {
        if (IsDead)
            return;

        CanChangeVelocity = canChange;
    }

    public void SetDead()
    {
        if (IsDead)
            return;

        IsDead = true;
        IsDashing = false;
        CanChangeVelocity = false;

        Debug.Log("Player state: DEAD");
    }
}