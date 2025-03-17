using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    // Existing variables
    [Header("Build Objects")]
    [SerializeField] private List<GameObject> floorobjects = new List<GameObject>();
    [SerializeField] private List<GameObject> wallobjectts = new List<GameObject>();
    [SerializeField] private List<GameObject> freeformObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> roofObjects = new List<GameObject>(); // Added for roof objects


    [Header("Build Settings")]
    [SerializeField] private SelectedBuildType currentBuildType;
    [SerializeField] private LayerMask connectedLayer;

    [Header("Particle Settings")]
    [SerializeField] private GameObject buildParticlePrefab;

    [Header("Ghost Settings")]
    [SerializeField] private Material ghostMaterialvalid;
    [SerializeField] private Material ghostMaterialInvalid;
    [SerializeField] private float connectorOverlapRadius = 1f;
    [SerializeField] private float maxgroundAngle = 45f;

    [Header("Internal State")]
    [SerializeField] private bool isbuilding = false;
    [SerializeField] private bool isDeleting = false;
    [SerializeField] private int currentBuildingIndex;
    private GameObject ghostbuildObject;
    private bool isGhostInValidPosistion = false;
    private Transform modelParent = null;
    private GameObject lastHighlightedBuilding;

    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();

    private float currentRotationAngle = 0f; // Store the current rotation angle of the object

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.B))
        {
            isbuilding = false;
           

            if (!isbuilding && ghostbuildObject)
            {
                Destroy(ghostbuildObject);
                ghostbuildObject = null;
            }


        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            isDeleting = !isDeleting;
            isbuilding = false;

            if (!isDeleting && ghostbuildObject)
            {
                Destroy(ghostbuildObject);
                ghostbuildObject = null;
            }

            if (!isDeleting && lastHighlightedBuilding != null)
            {
                ResetBuildingMaterial(lastHighlightedBuilding);
                lastHighlightedBuilding = null;
            }
        }

      if (Input.GetKeyDown(KeyCode.Alpha1))
{
    currentBuildType = SelectedBuildType.Floor;
    currentBuildingIndex = Mathf.Clamp(currentBuildingIndex, 0, floorobjects.Count - 1);
}

if (Input.GetKeyDown(KeyCode.Alpha2))
{
    currentBuildType = SelectedBuildType.Wall;
    currentBuildingIndex = Mathf.Clamp(currentBuildingIndex, 0, wallobjectts.Count - 1);
}

if (Input.GetKeyDown(KeyCode.Alpha3))
{
    currentBuildType = SelectedBuildType.FreeFrom;
    currentBuildingIndex = Mathf.Clamp(currentBuildingIndex, 0, freeformObjects.Count - 1);
}


        // Rotate the object using scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f && ghostbuildObject != null)
        {
            RotateObject(scrollInput);
        }

        if (isbuilding)
        {
            GhostBuild();
            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuild();
            }
        }

        if (isDeleting)
        {
            HighlightDeletingBuilding();
            if (Input.GetMouseButtonDown(1))
            {
                DeleteHighlightedBuilding();
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentBuildingIndex++;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && currentBuildingIndex > 0)
        {
            currentBuildingIndex--;
        }
    }

    // Handle the rotation using the scroll wheel (90 degrees increments)
    private void RotateObject(float scrollInput)
    {
        if (scrollInput > 0f) // Scroll Up
        {
            currentRotationAngle += 90f;
        }
        else if (scrollInput < 0f) // Scroll Down
        {
            currentRotationAngle -= 90f;
        }

        // Clamp the rotation angle between 0 and 360 degrees
        currentRotationAngle = Mathf.Repeat(currentRotationAngle, 360f);

        // Apply the rotation to the ghost object
        ghostbuildObject.transform.rotation = Quaternion.Euler(0f, currentRotationAngle, 0f);
    }

    private void GhostBuild()
    {
        GameObject currentBuild = GetCurrentBuild();
        CreateGhostPrefab(currentBuild);

        if (currentBuildType == SelectedBuildType.FreeFrom)
        {
            MoveGhostToRaycast();
            ghostifyModel(modelParent, ghostMaterialvalid);
            isGhostInValidPosistion = true;
        }
        else
        {
            MoveGhostToRaycast();
            checkBuildVadility();
        }
    }
    public void ActivateBuildMode()
    {
        isbuilding = true;
        isDeleting = false;

        if (lastHighlightedBuilding != null)
        {
            ResetBuildingMaterial(lastHighlightedBuilding);
            lastHighlightedBuilding = null;
        }

        Debug.Log("Build mode activated!");
    }
    private void CreateGhostPrefab(GameObject currentbuild)
    {
        if (currentbuild == null)
        {
            Debug.LogError("CreateGhostPrefab received a null object!");
            return;
        }

        if (ghostbuildObject == null)
        {
            ghostbuildObject = Instantiate(currentbuild);
            modelParent = ghostbuildObject.transform.GetChild(0);

            if (modelParent == null)
            {
                Debug.LogError("ModelParent is null. Make sure the prefab structure is correct.");
                return;
            }

            ghostifyModel(modelParent, ghostMaterialvalid);
            ghostifyModel(ghostbuildObject.transform);
        }
    }

    private void MoveGhostToRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            ghostbuildObject.transform.position = hit.point;
        }
    }

    private void checkBuildVadility()
    {
        Collider[] colliders = Physics.OverlapSphere(ghostbuildObject.transform.position, connectorOverlapRadius, connectedLayer);
        if (colliders.Length > 0)
        {
            ghostConnectedBuild(colliders);
        }
        else
        {
            ghostSeperateBuild();

            if (isGhostInValidPosistion)
            {
                Collider[] overlapColliders = Physics.OverlapBox(ghostbuildObject.transform.position, new Vector3(2f, 2f, 2f), ghostbuildObject.transform.rotation);
                foreach (Collider overlapcollider in overlapColliders)
                {
                    if (overlapcollider.gameObject != ghostbuildObject && (overlapcollider.transform.root.CompareTag("Buildables") || overlapcollider.transform.root.CompareTag("Furniture")))
                    {
                        ghostifyModel(modelParent, ghostMaterialInvalid);
                        isGhostInValidPosistion = false;
                        return;
                    }
                }
            }
        }
    }

    private void ghostConnectedBuild(Collider[] colliders)
    {
        Connector bestConnector = null;

        foreach (Collider collider in colliders)
        {
            Connector connector = collider.GetComponent<Connector>();

            if (connector.canConnectTo)
            {
                bestConnector = connector;
                break;
            }
        }

        if (bestConnector == null || currentBuildType == SelectedBuildType.Floor && bestConnector.isConnectedToFloor || currentBuildType == SelectedBuildType.Wall && bestConnector.isConnectedToWall)
        {
            ghostifyModel(modelParent, ghostMaterialvalid);
            isGhostInValidPosistion = false;
            return;
        }

        snapGhostPrefabToConnector(bestConnector);
    }

    private void snapGhostPrefabToConnector(Connector connector)
    {
        Transform ghostConnector = findSnapConnector(connector.transform, ghostbuildObject.transform.GetChild(1));

        if (ghostConnector == null)
        {
            Debug.LogError($"No valid snap connector found on {ghostbuildObject.name}");
            return;
        }

        ghostbuildObject.transform.position = connector.transform.position - (ghostConnector.position - ghostbuildObject.transform.position);
        if (currentBuildType == SelectedBuildType.Wall)
        { // Adjust ghost position relative to the connector
          

            // Get the rotation from the object the connector is on (only using the Y-axis)
            Quaternion connectorRotation = connector.transform.rotation; // or connector.transform.parent.rotation if needed
            float newYRotation = connectorRotation.eulerAngles.y;

            // Preserve the ghost object's X and Z rotation, but override its Y rotation
            Vector3 ghostEuler = ghostbuildObject.transform.rotation.eulerAngles;
            ghostbuildObject.transform.rotation = Quaternion.Euler(ghostEuler.x, newYRotation, ghostEuler.z);
        }

        ghostifyModel(modelParent, ghostMaterialvalid);
        isGhostInValidPosistion = true;
    }



    private bool IsWall(Vector3 normal)
    {
        // Check if the normal vector indicates the object is a wall
        // Assuming walls have normal vectors along the x or z axis, and floors have a normal of (0, 1, 0)
        return normal != Vector3.up;  // If the normal is not 'up' (i.e., (0, 1, 0)), consider it a wall
    }

    private void ghostSeperateBuild()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (currentBuildType == SelectedBuildType.Wall)
            {
                ghostifyModel(modelParent, ghostMaterialInvalid);
                isGhostInValidPosistion = false;
                return;
            }
            if (Vector3.Angle(hit.normal, Vector3.up) < maxgroundAngle)
            {
                ghostifyModel(modelParent, ghostMaterialvalid);
                isGhostInValidPosistion = true;
            }
            else
            {
                ghostifyModel(modelParent, ghostMaterialInvalid);
                isGhostInValidPosistion = false;
            }
        }
    }

    private Transform findSnapConnector(Transform snapConnector, Transform ghostConnectorParent)
    {
        ConnectorPositions OppisiteConnectorTag = getOppisitePosition(snapConnector.GetComponent<Connector>());

        foreach (Connector connector in ghostConnectorParent.GetComponentsInChildren<Connector>())
        {
            if (connector.connectorPosition == OppisiteConnectorTag)
            {
                return connector.transform;
            }
        }
        return null;
    }

    private ConnectorPositions getOppisitePosition(Connector connector)
    {
        ConnectorPositions positions = connector.connectorPosition;

        if (currentBuildType == SelectedBuildType.Wall && connector.connectorParent == SelectedBuildType.Floor)
        {
            return ConnectorPositions.bottom;
        }

        if (currentBuildType == SelectedBuildType.Floor
            && connector.connectorParent == SelectedBuildType.Wall
            && connector.connectorPosition == ConnectorPositions.top)
        {
            if (connector.transform.root.rotation.y == 0)
            {
                return getConnectorClosetsToPlayer(true);
            }
            else
            {
                return getConnectorClosetsToPlayer(false);
            }
        }

        switch (positions)
        {
            case ConnectorPositions.left:
                return ConnectorPositions.right;
            case ConnectorPositions.right:
                return ConnectorPositions.left;
            case ConnectorPositions.top:
                return ConnectorPositions.bottom;
            case ConnectorPositions.bottom:
                return ConnectorPositions.top;
            default:
                return ConnectorPositions.bottom;
        }
    }

    private ConnectorPositions getConnectorClosetsToPlayer(bool topBottom)
    {
        Transform cameratransfomr = Camera.main.transform;

        if (topBottom)
        {
            return cameratransfomr.position.z >= ghostbuildObject.transform.position.z ? ConnectorPositions.bottom : ConnectorPositions.top;
        }
        else
        {
            return cameratransfomr.position.x >= ghostbuildObject.transform.position.x ? ConnectorPositions.left : ConnectorPositions.right;
        }

    }

    private void ghostifyModel(Transform modelParent, Material ghostMaterial = null)
    {
        if (ghostMaterial != null)
        {
            foreach (MeshRenderer meshRenderer in modelParent.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = ghostMaterial;
            }
        }
        foreach (Collider modelcollider in modelParent.GetComponentsInChildren<Collider>())
        {
            modelcollider.enabled = false;
        }
    }

    private GameObject GetCurrentBuild()
    {
        switch (currentBuildType)
        {
            case SelectedBuildType.Floor:
                return floorobjects[currentBuildingIndex];
            case SelectedBuildType.Wall:
                return wallobjectts[currentBuildingIndex];
            case SelectedBuildType.Roof:  // Added Roof selection
                return roofObjects[currentBuildingIndex];
            case SelectedBuildType.FreeFrom:
                return freeformObjects[currentBuildingIndex];
        }
        return null;
    }


    private void PlaceBuild()
    {
        if (ghostbuildObject != null && (isGhostInValidPosistion || currentBuildType == SelectedBuildType.FreeFrom))
        {
            // Instantiate the building object at the ghost position and rotation
            GameObject newBuild = Instantiate(GetCurrentBuild(), ghostbuildObject.transform.position, ghostbuildObject.transform.rotation);

            // Instantiate the particle system at the same position where the build was placed
            InstantiateBuildParticle(ghostbuildObject.transform.position);

            // Destroy the ghost object
            Destroy(ghostbuildObject);
            ghostbuildObject = null;
        }
    }

    private void InstantiateBuildParticle(Vector3 position)
    {
        if (buildParticlePrefab != null)
        {
            // Instantiate the particle effect at the location where the build was placed
            Instantiate(buildParticlePrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Build particle prefab is not assigned!");
        }
    }


    private void HighlightDeletingBuilding()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Building"))
            {
                GameObject currentBuilding = hit.collider.gameObject;

                if (lastHighlightedBuilding != null && lastHighlightedBuilding != currentBuilding)
                {
                    ResetBuildingMaterial(lastHighlightedBuilding);
                }

                if (!originalMaterials.ContainsKey(currentBuilding))
                {
                    MeshRenderer[] renderers = currentBuilding.GetComponentsInChildren<MeshRenderer>();
                    Material[] mats = new Material[renderers.Length];

                    for (int i = 0; i < renderers.Length; i++)
                    {
                        mats[i] = renderers[i].material;
                    }

                    originalMaterials[currentBuilding] = mats;
                }

                foreach (MeshRenderer meshRenderer in currentBuilding.GetComponentsInChildren<MeshRenderer>())
                {
                    meshRenderer.material = ghostMaterialInvalid;
                }

                lastHighlightedBuilding = currentBuilding;
                return;
            }
        }

        if (lastHighlightedBuilding != null)
        {
            ResetBuildingMaterial(lastHighlightedBuilding);
            lastHighlightedBuilding = null;
        }
    }

    private void ResetBuildingMaterial(GameObject building)
    {
        if (originalMaterials.ContainsKey(building))
        {
            MeshRenderer[] renderers = building.GetComponentsInChildren<MeshRenderer>();
            Material[] originalMats = originalMaterials[building];

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = originalMats[i];
            }

            originalMaterials.Remove(building);
        }
    }

    private void DeleteHighlightedBuilding()
    {
        if (lastHighlightedBuilding != null)
        {
            GameObject rootObject = lastHighlightedBuilding.transform.root.gameObject;
            Destroy(rootObject);
            lastHighlightedBuilding = null;
        }
    }
    public void SetBuildType(SelectedBuildType newBuildType)
    {
        currentBuildType = newBuildType;
        currentBuildingIndex = Mathf.Clamp(currentBuildingIndex, 0, wallobjectts.Count - 1);
    }
    public void SetBuildIndex(int newIndex)
    {
        currentBuildingIndex = newIndex;
        Debug.Log("Build index set to: " + currentBuildingIndex);
    }

}
[System.Serializable]
public enum SelectedBuildType
{
    Floor,
    Wall,
    Roof,
    FreeFrom

}