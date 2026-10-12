using UnityEngine;

//every call is a safe no-op if animator/params are unassigned, so animations stay fully optional per enemy.
public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string attackTriggerFallback = "Attack";
    [SerializeField] private string attackIndexParam = "AttackIndex";
    [SerializeField] private string distanceToPlayerParam = "DistanceToPlayer";
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string deathTrigger = "Death";
    [SerializeField] private string stunnedBool = "Stunned";
    [SerializeField] private string hackedBool = "Hacked";

    public void SetMoving(bool isMoving)
    {
        if (animator == null) return;
        animator.SetFloat(speedParam, isMoving ? 1f : 0f);
    }

    public void SetDistanceToPlayer(float distance)
    {
        if (animator == null) return;
        animator.SetFloat(distanceToPlayerParam, distance);
    }

    //attackIndex is the attack's position in EnemyData's attack list, lets a Blend Tree/sub-state machine pick a specific attack animation by number instead of (or alongside) a named trigger.
    public void PlayAttack(string attackTrigger, int attackIndex)
    {
        if (animator == null) return;

        animator.SetInteger(attackIndexParam, attackIndex);

        string trigger = string.IsNullOrEmpty(attackTrigger) ? attackTriggerFallback : attackTrigger;
        if (!string.IsNullOrEmpty(trigger))
            animator.SetTrigger(trigger);
    }

    public void PlayHit()
    {
        if (animator == null) return;
        animator.SetTrigger(hitTrigger);
    }

    public void PlayDeath()
    {
        if (animator == null) return;
        animator.SetTrigger(deathTrigger);
    }

    public void SetStunned(bool isStunned)
    {
        if (animator == null) return;
        animator.SetBool(stunnedBool, isStunned);
    }

    public void SetHacked(bool isHacked)
    {
        if (animator == null) return;
        animator.SetBool(hackedBool, isHacked);
    }
}
