using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public List<Quest> activeQuests = new List<Quest>(); // List of currently active quests
    public int maxActiveQuests = 3; // Limit of active quests
    private QuestGiver currentQuestGiver;  // Reference to the current QuestGiver
    private Quest currentQuest;
    public AudioClip questCompleted;

    // --- New UI References for Quest Completion ---
    [Header("Quest Completed UI")]
    public GameObject questCompletedPanel;  // This panel will pop up when a quest is completed.
    public TMP_Text questCompletedText;       // This text field displays the completed quest name.

    // Add a new quest.
    public void AddQuest(Quest quest)
    {
        if (activeQuests.Count < maxActiveQuests)
        {
            activeQuests.Add(quest);
            Debug.Log("Quest added: " + quest.questName);
        }
        else
        {
            Debug.Log("You already have the maximum number of active quests.");
        }
    }

    // Remove a quest.
    public void RemoveQuest(Quest quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            Debug.Log("Quest removed: " + quest.questName);
        }
    }

    // Update progress of all active quests.
    void Update()
    {
        // Create a temporary list to store quests that are completed.
        List<Quest> questsToRemove = new List<Quest>();

        foreach (var quest in activeQuests)
        {
            quest.UpdateProgress();
            if (quest.isCompleted)
            {
                questsToRemove.Add(quest); // Mark quest for removal.
                Debug.Log("Quest completed: " + quest.questName);
                ShowQuestCompleted(quest);
            }
        }

        // Remove completed quests after iteration.
        foreach (var quest in questsToRemove)
        {
            RemoveQuest(quest);
        }
    }

    // Called when accepting a quest via QuestGiver.
    public void AcceptQuest()
    {
        if (currentQuest != null && currentQuestGiver != null)
        {
            currentQuestGiver.AcceptQuest();
            Debug.Log("Quest accepted: " + currentQuest.questName);
        }
    }

    // Set the current quest and QuestGiver when the player interacts with an NPC.
    public void SetCurrentQuestGiver(QuestGiver questGiver, Quest quest)
    {
        currentQuestGiver = questGiver;
        currentQuest = quest;
    }

    // Called when an enemy is killed.
    public void EnemyKilled(GameObject killedEnemy)
    {
        foreach (Quest quest in activeQuests)
        {
            if (quest.isAccepted && quest.questType == QuestType.Kill)
            {
                quest.Killed(killedEnemy);
                Debug.Log("Updated kill progress for quest: " + quest.questName);
            }
        }
    }

    // --- New method to show completed quest UI ---
    private void ShowQuestCompleted(Quest quest)
    {
        if (questCompletedPanel != null && questCompletedText != null)
        {
            AudioSource.PlayClipAtPoint(questCompleted, gameObject.transform.position, 2.5f);
            questCompletedText.text = "Quest Completed: " + quest.questName;
            questCompletedPanel.SetActive(true);
            // Start coroutine to hide the panel after a few seconds.
            StartCoroutine(HideQuestCompletedPanel());
        }
    }

    private IEnumerator HideQuestCompletedPanel()
    {
        yield return new WaitForSeconds(3f); // Display for 3 seconds.
        if (questCompletedPanel != null)
        {
            questCompletedPanel.SetActive(false);
        }
    }
}
