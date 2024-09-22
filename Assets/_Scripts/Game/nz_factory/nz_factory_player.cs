using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class nz_factory_player : MonoBehaviour
{
    public bool hasKeyCard;
    private bool performInteraction = false, interacting = false;
    private GameObject entryCard;
    // Start is called before the first frame update
    void Start()
    {
        InputManager.Instance.interact += nz_factory_manager.Instance.PickUp;
        InputManager.Instance.interact += PickUpEntryCard;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void PickUpEntryCard()
    {
        if (interacting && entryCard != null)
        {
            hasKeyCard = true;
            UIManager.Instance.interactText.gameObject.SetActive(true);
            UIManager.Instance.interactText.text = "Picked up a keycard";
            UIManager.Instance.interactText.GetComponent<Animator>().Play("interact_text_fadeout");
            Destroy(entryCard);
            interacting = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("EntryCard")) //Obtenci�n de keycard
        {
            entryCard = other.gameObject;
            interacting = true;
        }
        if (other.CompareTag("Llave")) //Comprueba que el jugador est� tocando la llave
        {
            nz_factory_manager.Instance.allowPickup = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EntryCard")) //Obtenci�n de keycard
        {
            entryCard = null;
            interacting = false;
        }
        if (other.CompareTag("Llave"))
        {
            nz_factory_manager.Instance.allowPickup = false;
        }
    }

        private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Pieza1") && nz_factory_manager.Instance.obtainedPickup == false && nz_factory_manager.Instance.isKeyObtained == true) //Comprueba si esta tocando la pieza 1 y asigna los valores debidos
        {
            nz_factory_manager.Instance.allowPickup = true;
            nz_factory_manager.Instance.selectedPart = 0;
        }
        else if (other.CompareTag("Pieza2") && nz_factory_manager.Instance.obtainedPickup == false && nz_factory_manager.Instance.isKeyObtained == true) //Comprueba si esta tocando la pieza 2 y asigna los valores debidos
        {
            nz_factory_manager.Instance.allowPickup = true;
            nz_factory_manager.Instance.selectedPart = 1;
        }
        else if (other.CompareTag("Pieza3") && nz_factory_manager.Instance.obtainedPickup == false && nz_factory_manager.Instance.isKeyObtained == true) //Comprueba si esta tocando la pieza 3 y asigna los valores debidos
        {
            nz_factory_manager.Instance.allowPickup = true;
            nz_factory_manager.Instance.selectedPart = 2;

        }
        if (other.CompareTag("Llave") && nz_factory_manager.Instance.isKeyObtained == false) //Comprueba si esta tocando la llave y asigna los valores debidos
        {
            nz_factory_manager.Instance.allowPickup = true;
            nz_factory_manager.Instance.selectedPart = 3;
        }


    }
}
