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
        sensitivity = PlayerPrefs.GetFloat("sensitivity", .5f);
        sensitivitySlider.value = sensitivity;
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
        yRotation += (InputManager.Instance._inputActions.Player.MouseLook.ReadValue<Vector2>().x + InputManager.Instance._inputActions.Player.ControllerLook.ReadValue<Vector2>().x * 2) * sensitivity;
        xRotation += (InputManager.Instance._inputActions.Player.MouseLook.ReadValue<Vector2>().y + InputManager.Instance._inputActions.Player.ControllerLook.ReadValue<Vector2>().y * 2) * sensitivity;

        transform.rotation = Quaternion.Euler(0, yRotation, 0);
        playerCam.transform.rotation = Quaternion.Euler(-xRotation, yRotation, 0);
        xRotation = Mathf.Clamp(xRotation, clampMin, clampMax);

    }
    public void UpdateSensitivty()
    {
        sensitivity = sensitivitySlider.value;
    }
}

