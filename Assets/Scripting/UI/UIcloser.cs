using UnityEngine;

public class UIcloser : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Close()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1;
    }
}
