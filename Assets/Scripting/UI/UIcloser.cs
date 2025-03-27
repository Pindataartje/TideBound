using UnityEngine;

public class UIcloser : MonoBehaviour
{
    public void Close()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1;
    }
}
