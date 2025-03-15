using System.Collections;
using UnityEngine;

[ScriptTag("Item")]
public class Spear : MonoBehaviour
{
    public Animator animator;       // Assign your Animator component in the Inspector.
    public float attackDuration = 1f; // Duration of the attack animation in seconds.
    private bool canAttack = true;

    void Update()
    {
        // On left mouse button down, if we are allowed to attack...
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            Attack();
        }
    }

    void Attack()
    {
        // Trigger the "Attack" animation.
        animator.SetTrigger("Attack");
        // Disable further attacks.
        canAttack = false;
        // Start a coroutine to re-enable attacks after the animation finishes.
        StartCoroutine(ResetAttack());
    }

    IEnumerator ResetAttack()
    {
        // Wait for the duration of the attack animation.
        yield return new WaitForSeconds(attackDuration);
        canAttack = true;
    }
}
