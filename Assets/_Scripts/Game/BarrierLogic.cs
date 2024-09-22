using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Este script gestiona la lógica de las barreras defensivas en el juego.
    Controla el estado de salud de la barrera, la destrucción y reparación de partes de la misma.
*/

public class BarrierLogic : MonoBehaviour
{
    public int hitPoints;
    public GameObject[] planks;

    // Reduce los puntos de resistencia de la barrera y la destruye si alcanza cero puntos
    public void ReduceHitPoints()
    {
        hitPoints--;
        DestroyBarrier();
    }

    // Destruye la parte de la barrera correspondiente a los puntos de resistencia actuales
    public void DestroyBarrier()
    {
        planks[hitPoints].SetActive(false);
        if (hitPoints <= 0)
        {
            transform.GetChild(0).gameObject.SetActive(false); // Desactiva el objeto padre si la barrera está completamente destruida
        }
    }

    // Repara la barrera, activando la parte destruida y aumentando los puntos de resistencia
    public void RepairBarrier()
    {
        if (hitPoints <= 0)
        {
            transform.GetChild(0).gameObject.SetActive(true); // Activa la barrera completa si estaba completamente destruida
        }
        planks[hitPoints].SetActive(true);
        hitPoints++;
        GameManager.Instance.AddPoints(10);
    }
}

