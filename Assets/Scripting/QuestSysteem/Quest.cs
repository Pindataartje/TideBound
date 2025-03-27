using JetBrains.Annotations;
using System;
using UnityEngine;

public enum QuestType
{
    Kill,
    Gather,
    TakeOver
}

[Serializable]
public class Quest
{
    public string questName;
    public string questDescription;
    public QuestType questType;
    public int targetAmount;  // How many enemies to kill or items to gather
    public int currentAmount; // Current progress on quest

    // For Gather quests, using a tag rather than an item name.
    public string targetItemTag;

    // For Kill quests, we now use a target enemy tag instead of a target GameObject.
    public string targetEnemyTag = "Enemy";

    public bool isAccepted; // Only update progress when quest is accepted
    public bool isCompleted;

    // This method simply checks whether the quest is complete.
    public void UpdateProgress()
    {
        if (!isAccepted)
            return;

        if (currentAmount >= targetAmount)
        {
            isCompleted = true;
        }
    }

    // Called when an enemy is killed. Checks that the killed enemy has the correct tag.
    public void Killed(GameObject killedEnemy)
    {
        if (!isAccepted || questType != QuestType.Kill)
            return;

        // Use CompareTag instead of directly accessing the tag property
        if (killedEnemy.TryGetComponent<TagAssigner>(out TagAssigner tagAssigner))
        {
            if (tagAssigner.customTag == targetEnemyTag)
            {
                currentAmount++;
                if (currentAmount >= targetAmount)
                {
                    isCompleted = true;
                }
            }
        }
        else if (killedEnemy.CompareTag(targetEnemyTag))
        {
            currentAmount++;
            if (currentAmount >= targetAmount)
            {
                isCompleted = true;
            }
        }
    }

    // Called when a gatherable item is collected.
    public void CollectedItem(GameObject item)
    {
        if (!isAccepted || questType != QuestType.Gather)
            return;

        if (item.CompareTag(targetItemTag))
        {
            currentAmount++;
            if (currentAmount >= targetAmount)
            {
                isCompleted = true;
            }
        }
    }
}
