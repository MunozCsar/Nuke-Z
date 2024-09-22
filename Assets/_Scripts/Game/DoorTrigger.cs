using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
    Este script controla el comportamiento de un disparador de puerta que se activa cuando el jugador se acerca a �l.
    Muestra un texto interactivo y permite al jugador abrir la puerta con una tecla espec�fica si tiene una tarjeta clave.
*/

public class DoorTrigger : MonoBehaviour
{
    public bool triggerOn = false;
    public Animator doorAnim;

    private void Start()
    {
        InputManager.Instance.interact += OpenDoor;
    }

    // Se llama cuando un objeto entra en el �rea de colisi�n del disparador
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(("Player")))
        {
            UIManager.Instance.interactText.GetComponent<Animator>().Play("interact_text_idle");
            if (other.GetComponent<nz_factory_player>().hasKeyCard.Equals(true))
            {
                triggerOn = true;
                UIManager.Instance.interactText.gameObject.SetActive(true);
                UIManager.Instance.interactText.gameObject.GetComponent<Animator>().Play("interact_text_idle");
                UIManager.Instance.interactText.text = ("Press \"F\" to insert Keycard");
            }
            else
            {
                UIManager.Instance.interactText.gameObject.SetActive(true);
                UIManager.Instance.interactText.gameObject.GetComponent<Animator>().Play("interact_text_idle");
                UIManager.Instance.interactText.text = ("You need a keycard to open this door");
            }
        }
    }

    // Se llama cuando un objeto sale del �rea de colisi�n del disparador
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(("Player")))
        {
            triggerOn = false;
            UIManager.Instance.interactText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Verifica si se presiona la tecla correspondiente para abrir la puerta y si el disparador est� activado

    }

    private void OpenDoor()
    {
        if (triggerOn)
        {
            UIManager.Instance.interactText.gameObject.SetActive(false);
            doorAnim.SetTrigger("Fold");
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}

