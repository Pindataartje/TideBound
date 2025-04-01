using UnityEngine;

public class MaterialHealth : MonoBehaviour
{
    public float healht;



    public void TookDamage(float damage)
    {
        healht -= damage;
            if (healht <= 0f)
            {
             Destroy (gameObject);
            }
    }

    
}
