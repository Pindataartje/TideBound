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
    public AudioClip stoneHit;
    public AudioClip metalHit;
    public AudioSource audioSource; // AudioSource component to play sounds

    private void Start()
    {
        // Get the WeaponHandler from the parent or another GameObject
        weaponHandler = GetComponentInParent<WeaponHandler>();

        // Ensure there's an AudioSource component attached
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
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
                animalAI.TakeDamage(10f);
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
                PlaySoundBasedOnTag(itemTag);
            }
            else
            {
                Debug.Log("Material hit but has no TagAssigner component.");
            }
        }
    }

    private void PlaySoundBasedOnTag(string tag)
    {
        switch (tag.ToLower())
        {
            case "wood":
                audioSource.PlayOneShot(woodChop);
                break;
            case "stone":
                audioSource.PlayOneShot(stoneHit);
                break;
            case "metal":
                audioSource.PlayOneShot(metalHit);
                break;
            default:
                Debug.Log("No specific sound assigned for tag: " + tag);
                break;
        }
    }
}
