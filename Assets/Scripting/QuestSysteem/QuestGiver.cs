using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestGiver : MonoBehaviour
{
    public QuestManager questManager;  // Reference to the QuestManager
    public Quest[] availableQuests;   // Array of quests that the NPC can give
    public GameObject questUIPanel;   // The UI panel that contains quest name, description, and accept button
    public TMP_Text questNameText;    // TMP text field for displaying the quest name
    public TMP_Text questDescriptionText;  // TMP text field for displaying the quest description
    public AudioClip acceptQuest;

    private Quest currentQuest;      // The current quest the player will accept
    private int nextQuestIndex = 0;  // Tracks the next quest to be given

    private void Start()
    {
        if (questManager == null)
        {
            questManager = FindAnyObjectByType<QuestManager>();

            if (questManager == null)
            {
                Debug.LogWarning("QuestManager not found in the scene. Make sure it exists.");
            }
        }

        // Ensure the quest UI is hidden at the start
        questUIPanel.SetActive(false);
    }

    // Player interacts with NPC to receive a quest
    public void Interact()
    {
        if (nextQuestIndex < availableQuests.Length)
        {
            GiveQuest(nextQuestIndex);
            Cursor.lockState = CursorLockMode.Confined;
            
        }
        else
        {
            Debug.Log("No more quests available from this QuestGiver.");
        }
    }

    // Give a quest to the player and update the UI
    private void GiveQuest(int questIndex)
    {
        if (questIndex >= 0 && questIndex < availableQuests.Length)
        {
            currentQuest = availableQuests[questIndex];

            // Set the UI text fields with the quest information
            questNameText.text = currentQuest.questName;
            questDescriptionText.text = currentQuest.questDescription;

            // Show the quest UI and stop time
            OpenQuestUI();

            // Set the QuestManager's current quest and QuestGiver reference
            questManager.SetCurrentQuestGiver(this, currentQuest);
        }
        else
        {
            Debug.Log("Invalid quest index.");
        }
    }

    // Function to open the UI and stop time
    private void OpenQuestUI()
    {
        questUIPanel.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Function to close the UI and resume time
    private void CloseQuestUI()
    {
        questUIPanel.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Called when the player clicks the accept button
    public void AcceptQuest()
    {
        if (currentQuest != null)
        {
            // Set the quest as accepted so progress can start increasing.
            currentQuest.isAccepted = true;

            // Add the quest to the QuestManager
            questManager.AddQuest(currentQuest);

            // Hide the quest UI and resume time after accepting the quest
            CloseQuestUI();
            Debug.Log("Quest accepted: " + currentQuest.questName);
            AudioSource.PlayClipAtPoint(acceptQuest, gameObject.transform.position);
        }
    }

    // Called when a quest is completed, sets up the next quest but doesn't give it immediately
    public void OnQuestCompleted(Quest completedQuest)
    {
        // Find the index of the completed quest
        int questIndex = System.Array.IndexOf(availableQuests, completedQuest);

        // Check if there's another quest available after this one
        if (questIndex >= 0 && questIndex < availableQuests.Length - 1)
        {
            nextQuestIndex = questIndex + 1;
            Debug.Log("Next quest is ready. Interact with NPC to receive it.");
        }
        else
        {
            Debug.Log("No more quests available from this QuestGiver.");
        }
    }
}
