using System.Collections.Generic;
using UnityEngine;

public class Korte_Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float jumpHeight = 5f;

    [Header("Mouse Settings")]
    public float lookSensitivity = 100f;
    public Transform cameraTransform;

    [Header("Jumpable Layers")]
    public LayerMask jumpableLayers;

    [Header("Interaction Settings")]
    public float interactDistance = 3f;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool onFloor;

    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        MouseLook();
        Movement();
        HandleInteraction();
       
    }

    void FixedUpdate()
    {
        if (Input.GetButtonDown("Jump") && onFloor)
        {
            Jump();
        }
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Movement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        Vector3 velocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(move.x * walkSpeed, velocity.y, move.z * walkSpeed);
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsInLayerMask(collision.gameObject.layer, jumpableLayers))
        {
            onFloor = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (IsInLayerMask(collision.gameObject.layer, jumpableLayers))
        {
            onFloor = false;
        }
    }

    bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) > 0;
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactDistance))
            {
                if (hit.collider.CompareTag("Npc"))
                {
                    // Get the QuestGiver component from the NPC
                    QuestGiver questGiver = hit.collider.GetComponent<QuestGiver>();

                    if (questGiver != null)
                    {
                        // Call Interact instead of forcing a specific quest
                        questGiver.Interact();
                    }
                }
            }
        }
    }



}
