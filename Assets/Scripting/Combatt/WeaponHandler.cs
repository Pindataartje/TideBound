using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ScriptTag("Item")]
public class WeaponHandler : MonoBehaviour
{
    public GameObject weapon;  // The weapon GameObject
    public float attackCooldown = 1f;  // Cooldown time in seconds

    private ColliderHandeling colliderHandeling;  // Reference to ColliderHandeling
    private HashSet<Collider> hitEnemies;
    private float lastAttackTime = -Mathf.Infinity;  // Time of the last attack

    private void Start()
    {
        // Find the ColliderHandeling script in children
        colliderHandeling = GetComponentInChildren<ColliderHandeling>();

        if (colliderHandeling == null)
        {
            Debug.LogError("WeaponHandler: No ColliderHandeling component found on child objects!");
        }

        hitEnemies = new HashSet<Collider>();
    }

    private void Update()
    {
        // Check if enough time has passed since the last attack
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    public void Attack()
    {
        // Record the time of this attack
        lastAttackTime = Time.time;

        Animator anim = weapon.GetComponent<Animator>();
        anim.SetTrigger("Attack");

        if (colliderHandeling != null)
        {
            colliderHandeling.EnableCollider(); // Enable collider when attacking
        }

        hitEnemies.Clear();

        // Start the cooldown (no need for a coroutine now)
        if (colliderHandeling != null)
        {
            StartCoroutine(DisableColliderAfterCooldown());
        }
    }

    private IEnumerator DisableColliderAfterCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);

        if (colliderHandeling != null)
        {
            colliderHandeling.DisableCollider(); // Disable collider after cooldown
        }
    }

    public void AddHitEnemy(Collider enemy)
    {
        hitEnemies.Add(enemy);
    }

    public bool IsEnemyHit(Collider enemy)
    {
        return hitEnemies.Contains(enemy);
    }
}
