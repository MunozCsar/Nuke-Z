using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Este script gestiona el movimiento del cami�n cuando se coloca la llave.
*/

public class MoveTruck : MonoBehaviour
{
    public nz_factory_player player; // Referencia al controlador del jugador
    public bool canPlaceKey = false, move; // Indica si el cami�n puede recibir la llave
    public Transform target; // Posici�n a la que se mueve el cami�n
    public float speed; // Velocidad de movimiento del cami�n

    private void Start()
    {
        InputManager.Instance.interact += UseKey;
    }

    // Se llama en cada frame
    private void Update()
    {
        if (move)
        {
            Move();
        }
    }

    // Utiliza la llave para mover el cami�n si se cumplen las condiciones
    private void UseKey()
    {
        if (nz_factory_manager.Instance.isKeyObtained == true && nz_factory_manager.Instance.allowPickup == true && nz_factory_manager.Instance.isKeyActive == false)
        {
            if (canPlaceKey == true)
            {
                nz_factory_manager.Instance.isKeyActive = true;
            }
        }
        if (nz_factory_manager.Instance.isKeyActive)
        {
            move = true;
        }
    }

    // Mueve el cami�n hacia la posici�n objetivo
    public void Move()
    {
            float d = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.position, d);

    }

    // Se llama cuando un objeto permanece en el �rea de colisi�n
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            nz_factory_manager.Instance.allowPickup = true;

            if (nz_factory_manager.Instance.isKeyObtained == true)
            {
                canPlaceKey = true;
            }
        }
    }

    // Se llama cuando un objeto sale del �rea de colisi�n
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            canPlaceKey = false;
            nz_factory_manager.Instance.allowPickup = false;
        }
    }
}

