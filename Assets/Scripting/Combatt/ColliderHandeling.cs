using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ScriptTag("Item")]
public class ColliderHandeling : MonoBehaviour
{
    private WeaponHandler weaponHandler;
    public string itemTag;
    public InventoryItemAdder inventoryItemAdder;

    [Header("Sounds")]
    public AudioClip woodChop;

    private Collider objectCollider;

    private void Start()
    {
        // Get the WeaponHandler from the parent or another GameObject
        weaponHandler = GetComponentInParent<WeaponHandler>();

        // Get the collider on the same GameObject as this script
        objectCollider = GetComponent<Collider>();

        if (objectCollider != null)
        {
            objectCollider.isTrigger = true; // Ensure it's set as a trigger
        }
        else
        {
            Debug.LogWarning("No Collider found on " + gameObject.name + ". ColliderHandling requires a Collider.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debugging to check what collider is triggering the weapon
        Debug.Log("Collider Triggered by: " + other.gameObject.name);

        // Check if the collider belongs to an enemy and it's not already in the hit list
        if (other.CompareTag("Enemy") && !weaponHandler.IsEnemyHit(other))
        {
            AnimalAI animalAI = other.GetComponent<AnimalAI>();
            if (animalAI != null)
            {
                animalAI.TakeDamage(10f);  // Example damage value
                Debug.Log("Damage applied to " + animalAI.gameObject.name);

                // Add the enemy to the hit list to prevent re-hitting until the cooldown
                weaponHandler.AddHitEnemy(other);
            }
        }
        else if (other.CompareTag("Material"))
        {
            // Get the TagAssigner component from the collided object
            TagAssigner tagAssigner = other.GetComponent<TagAssigner>();

            if (tagAssigner != null)
            {
                Debug.Log("Material hit with assigned tag: " + tagAssigner.tagToAssign);
                itemTag = tagAssigner.tagToAssign;
                inventoryItemAdder.AddItemByTag(itemTag, 2);
                AudioSource.PlayClipAtPoint(woodChop, gameObject.transform.position);

            }
            else
            {
                Debug.Log("Material hit but has no TagAssigner component.");
            }


            MaterialHealth Mathealth = other.GetComponent<MaterialHealth>();
                if (Mathealth != null)
            {
                Mathealth.TookDamage(1);
            }
        }
    }

    public void EnableCollider()
    {
        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }
    }

    public void DisableCollider()
    {
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }
    }

}
