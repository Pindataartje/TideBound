using UnityEngine;

public class MeshColliderFollower : MonoBehaviour
{
    public GameObject mainObject;  // Assign the boat (main object) here
    public GameObject colliderObject;  // Assign the object with mesh colliders here

    private void Update()
    {
        if (mainObject != null && colliderObject != null)
        {
            // Follow the main object's position, rotation, and scale
            colliderObject.transform.position = mainObject.transform.position;
            colliderObject.transform.rotation = mainObject.transform.rotation;
            colliderObject.transform.localScale = mainObject.transform.localScale;
        }
    }
}
