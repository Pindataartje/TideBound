using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ScriptTag("Item")]
public class WeaponHandler : MonoBehaviour
{
    public GameObject weapon;  // The weapon GameObject with the collider
    public bool canAttack = true;
    public float attackCooldown = 1f;

    private Collider weaponCollider;
    private HashSet<Collider> hitEnemies;  // Set to track enemies that have been hit


    [Header ("sounds")]
    public AudioClip woodChop;

    private void Start()
    {
        // Get the collider attached to the weapon
        weaponCollider = weapon.GetComponent<Collider>();
        if (weaponCollider != null)
        {
            // Ensure the weapon's collider is set as a trigger (if it's not already set)
            weaponCollider.isTrigger = true;
        }

        hitEnemies = new HashSet<Collider>();  // Initialize the HashSet
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            Attack();
        }
    }

    public void Attack()
    {
        canAttack = false;
        Animator anim = weapon.GetComponent<Animator>();
        anim.SetTrigger("Attack");

        // Ensure the weapon's collider is enabled when attacking
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }

        // Clear the hit enemies set for the new attack
        hitEnemies.Clear();

        StartCoroutine(ResetAttackCooldown());
    }

    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);

        // Disable the weapon's collider after the attack cooldown period
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }

        canAttack = true;
    }

    // Public method to add an enemy to the hit list
    public void AddHitEnemy(Collider enemy)
    {
        hitEnemies.Add(enemy);
    }

    // Public method to check if the enemy is already in the hit list
    public bool IsEnemyHit(Collider enemy)
    {
        return hitEnemies.Contains(enemy);
    }
}
