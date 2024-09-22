using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nz_factory_manager : MonoBehaviour
{
    public int selectedPart = 4;
    public GameObject[] powerParts;
    public GameObject[] firstFloorLights, secondFloorLights;
    public Material lightMaterial;
    public GameObject[] radiationContainers;
    public GameObject truckKey, electricDoor;
    public bool[] radContainerFull;
    public bool isKeyActive = false, isKeyObtained = false, allowPickup, obtainedPickup, placeablePart, endGameTrigger;

    public static nz_factory_manager Instance { get; private set; }
    private void Awake() //Al iniciar el juego, se comprueba si ya existe una instancia del GameManager, si no la hay, este objeto se vuelve la instancia, si la hay, se destruye este objeto.
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PickUp()
    {
        //Gestiona la interaccion con la llave y las piezas

        if (allowPickup == true && obtainedPickup == false) 
        {
                if(selectedPart == 3)
               {
                    isKeyObtained = true;
                    truckKey.SetActive(false);
                }
                else if(selectedPart == 0 ||selectedPart == 1 || selectedPart == 2) //Si selecciona una pieza de electricidad, obtiene el pickup y la desactiva
                {
                    powerParts[selectedPart].SetActive(false);
                    obtainedPickup = true;
                }          
        }
    }

    public void CheckContainerCompletion(int i) //Comprueba si los contenedores est�n llenos, y si lo est�n, activa el trigger que permite finalizar la partida
    {
        if (radiationContainers[i].GetComponent<SoulHarvest>().actualSouls >= 20)
        {
            radContainerFull[i] = true;
        }
        else
        {
        }

        if (radContainerFull[0].Equals(true) && radContainerFull[1].Equals(true) && radContainerFull[2].Equals(true))
        {
            endGameTrigger = true;
        }
    }
}
