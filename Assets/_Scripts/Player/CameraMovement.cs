using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

/*
    Este script controla el movimiento de la c�mara en el juego, permitiendo al jugador mirar alrededor con el rat�n.
*/

public class CameraMovement : MonoBehaviour
{
    public GameObject playerCam;
    public Slider sensitivitySlider;

    public float clampMin, clampMax, sensitivity;
    float xRotation;
    float yRotation;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Establece la rotaci�n inicial
        xRotation = transform.eulerAngles.x;
        yRotation = transform.eulerAngles.y;
    }
    
    private void Start()
    {
        UpdateSensitivty();
    }

    void Update()
    {
        if (!GameManager.Instance.isPaused)
        {
            FPSCamLook();
        }

    }

    public void FPSCamLook()
    {

        yRotation += InputManager.Instance.GetCamX() * sensitivity;

        xRotation += InputManager.Instance.GetCamY() * sensitivity;

        transform.rotation = Quaternion.Euler(0, yRotation, 0);
        playerCam.transform.rotation = Quaternion.Euler(-xRotation, yRotation, 0);
        xRotation = Mathf.Clamp(xRotation, clampMin, clampMax);

    }
    public void UpdateSensitivty()
    {
        sensitivity = sensitivitySlider.value;
    }
}

