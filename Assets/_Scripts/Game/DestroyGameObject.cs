using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Este script destruye el GameObject al que está adjunto después de un cierto período de tiempo.
*/

public class DestroyGameObject : MonoBehaviour
{
    public int time;

    void Start()
    {
        // Destruye el GameObject después del tiempo especificado
        Destroy(gameObject, time);
    }
}

