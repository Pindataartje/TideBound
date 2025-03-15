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

    private Quest currentQuest;      // The current quest the player will accept

    private void Start()
    {
        if (questManager == null)
        {
            questManager = FindAnyObjectByType<QuestManager>(); // More efficient in newer Unity versions

            if (questManager == null)
            {
                Debug.LogWarning("QuestManager not found in the scene. Make sure it exists.");
            }
        }

        // Ensure the quest UI is hidden at the start
        questUIPanel.SetActive(false);
    }
     



    // Give a quest to the player and update the UI
    public void GiveQuest(int questIndex)
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
        questUIPanel.SetActive(true);  // Show the quest UI
        Time.timeScale = 0;  // Pause the game by setting time scale to 0
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Function to close the UI and resume time
    private void CloseQuestUI()
    {
        questUIPanel.SetActive(false);  // Hide the quest UI
        Time.timeScale = 1;  // Resume the game by setting time scale to 1
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
        }
    }
}
