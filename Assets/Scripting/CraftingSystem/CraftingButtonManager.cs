using UnityEngine;

public class CraftingButtonManager : MonoBehaviour
{
    public GameObject[] craftButtons;
    public void SelectButton(GameObject selectedButton)
    {
        foreach (GameObject btn in craftButtons)
        {
            if (btn == selectedButton)
            {
                btn.SetActive(true);
            }
            else
            {
                btn.SetActive(false);
            }
        }
    }
}
