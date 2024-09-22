using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public PlayerInput _inputActions;

    public Action shoot, autoShoot, ads;

    public Action reload;

    public Action jump;

    public Action interact;

    public Action flashLight;

    public Action sprint;

    public Action changeWeapon;

    public Action melee;

    public Action pause;

    public Action scoreBoard;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _inputActions = new PlayerInput();
        _inputActions.Enable();
    }

    private void Start()
    {
        _inputActions.Player.Jump.performed += OnJumpPerformed;
        _inputActions.Player.Interact.performed += OnInteractPerformed;
        _inputActions.Player.Flashlight.performed += OnFlashLightOn;
        _inputActions.Player.ChangeWeapon.performed += OnWeaponChanged;
        _inputActions.Player.Melee.performed += OnMeleePerformed;
        _inputActions.Player.Pause.performed += OnPausePerformed;

    }

    public float GetCamX()
    {
        return _inputActions.Player.CameraMovementX.ReadValue<float>();
    }
    public float GetCamY()
    {
        return _inputActions.Player.CameraMovementY.ReadValue<float>();
    }

    public Vector2 GetMovementDirection()
    {
        return _inputActions.Player.Movement.ReadValue<Vector2>();
    }

    private void OnShootPerformed(InputAction.CallbackContext obj)
    {
        shoot.Invoke();
    }


    private void OnJumpPerformed(InputAction.CallbackContext obj)
    {
        jump.Invoke();
    }

    private void OnInteractPerformed(InputAction.CallbackContext obj)
    {
        interact.Invoke();
    }

    private void OnFlashLightOn(InputAction.CallbackContext obj)
    {
        flashLight.Invoke();
    }

    private void OnWeaponChanged(InputAction.CallbackContext obj)
    {
        changeWeapon.Invoke();
    }

    private void OnMeleePerformed(InputAction.CallbackContext obj)
    {
        melee.Invoke();
    }
    private void OnPausePerformed(InputAction.CallbackContext obj)
    {
        pause.Invoke();
    }

}
