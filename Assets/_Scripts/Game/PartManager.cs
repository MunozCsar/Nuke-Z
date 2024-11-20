using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Este script gestiona las partes de la electricidad y la activaci�n de la energ�a el�ctrica en el nivel.
*/

public class PartManager : MonoBehaviour
{
    [SerializeField] GameObject[] parts;
    public int activeParts = 0;
    public bool powerUnlocked;

    private void Start()
    {
        InputManager.Instance.interact += PlaceObject;
        InputManager.Instance.interact += ActivatePower;
    }

    // Coloca la parte seleccionada si las condiciones son correctas
    private void PlaceObject()
    {
        if (nz_factory_manager.Instance.allowPickup == true && nz_factory_manager.Instance.placeablePart == true && nz_factory_manager.Instance.obtainedPickup == true)
        {
            if ((nz_factory_manager.Instance.selectedPart == 0 || nz_factory_manager.Instance.selectedPart == 1 || nz_factory_manager.Instance.selectedPart == 2))
            {
                parts[nz_factory_manager.Instance.selectedPart].SetActive(true);
                activeParts++;
                nz_factory_manager.Instance.selectedPart = 4;
                nz_factory_manager.Instance.obtainedPickup = false;
                nz_factory_manager.Instance.placeablePart = false;
            }
        }
    }

    // Activa la energ�a el�ctrica si se cumple la condici�n y se presiona la tecla correspondiente
    public void ActivatePower()
    {
        if (powerUnlocked)
        {
            foreach(GameObject light in nz_factory_manager.Instance.lights)
            {
                light.SetActive(true);
            }
            nz_factory_manager.Instance.lightMaterial.EnableKeyword("_EMISSION"); // Al activar la electricidad, activa la propiedad de emission del material de las luces
            nz_factory_manager.Instance.electricDoor.GetComponent<Animator>().SetTrigger("Fold");
            powerUnlocked = false;
            UIManager.Instance.interactText.gameObject.SetActive(false);
            GameManager.Instance.powerOn = true;
        }
    }

    // Se llama cuando un objeto permanece en el �rea de colisi�n
    private void OnTriggerStay(Collider other)
    {
        // Si detecta al jugador y tiene un pickup le permite colocar la parte en su lugar
        if (other.CompareTag("Player") && nz_factory_manager.Instance.obtainedPickup == true)
        {
            nz_factory_manager.Instance.placeablePart = true;
            nz_factory_manager.Instance.allowPickup = true;
        }
        // Si todas las partes est�n colocadas y la energ�a no est� activa, permite activar la energ�a
        if (activeParts == 3 && GameManager.Instance.powerOn == false)
        {
            UIManager.Instance.interactText.gameObject.SetActive(true);
            UIManager.Instance.interactText.GetComponent<Animator>().Play("interact_text_idle");
            UIManager.Instance.interactText.text = "Press \"F\" to turn on power";
            powerUnlocked = true;
        }
    }

    // Se llama cuando un objeto sale del �rea de colisi�n
    private void OnTriggerExit(Collider other)
    {
        // Si el jugador sale del �rea no le permite colocar la parte y desactiva el texto de interacci�n
        if (other.CompareTag("Player") && nz_factory_manager.Instance.obtainedPickup == true)
        {
            nz_factory_manager.Instance.placeablePart = false;
            nz_factory_manager.Instance.allowPickup = false;
            UIManager.Instance.interactText.gameObject.SetActive(false);
        }
    }
}
