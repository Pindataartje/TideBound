using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingUi : MonoBehaviour
{
    public BuildingManager bmanager;


   

    public void SetBuildObject()
    {
        if (bmanager != null)
        {
            GameObject clickedButton = EventSystem.current.currentSelectedGameObject;

            if (clickedButton != null)
            {
                BUII buiiComponent = clickedButton.GetComponent<BUII>();

                if (buiiComponent != null)
                {
                    int newIndex = buiiComponent.Index;  // Get index from button component
                    bmanager.SetBuildIndex(newIndex);   // Update the current building object index
                }
                else
                {
                    Debug.LogWarning("BUII component not found on clicked button!");
                }
            }
            else
            {
                Debug.LogWarning("No button selected in EventSystem!");
            }
        }
    }


    public void SetBuildIndex()
    {
        if (bmanager != null)
        {
            // Get the currently clicked button
            GameObject clickedButton = EventSystem.current.currentSelectedGameObject;

            if (clickedButton != null)
            {
                // Try to get the BUII component from the clicked button
                BUII buiiComponent = clickedButton.GetComponent<BUII>();

                if (buiiComponent != null)
                {
                    // Map the integer index to the corresponding SelectedBuildType
                    SelectedBuildType newBuildType;
                    switch (buiiComponent.Index)
                    {
                        case 1:
                            newBuildType = SelectedBuildType.Floor;
                            break;
                        case 2:
                            newBuildType = SelectedBuildType.Wall;
                            break;
                        case 3:
                            newBuildType = SelectedBuildType.Roof;
                            break;
                        case 4:
                            newBuildType = SelectedBuildType.FreeFrom;
                            break;
                        default:
                            Debug.LogWarning("Invalid build type index: " + buiiComponent.Index);
                            return;
                    }

                    // Set the build type in the BuildingManager
                    bmanager.SetBuildType(newBuildType);
                }
                else
                {
                    Debug.LogWarning("BUII component not found on clicked button!");
                }
            }
            else
            {
                Debug.LogWarning("No button selected in EventSystem!");
            }
        }
    }


}
