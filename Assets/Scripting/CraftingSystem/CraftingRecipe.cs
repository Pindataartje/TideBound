using UnityEngine;

[CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [System.Serializable]
    public struct ResourceRequirement
    {
        public string resourceTag; // e.g. "Stick"
        public int amount;         // e.g. 4
    }

    public string recipeName;
    public ResourceRequirement[] requirements;
    public GameObject outputPrefab; // e.g. the WoodenPlank prefab.
}
