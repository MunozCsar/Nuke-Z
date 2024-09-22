using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverbActivator : MonoBehaviour
{
    public GameObject reverbZone;

    private void Start()
    {
        reverbZone.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            reverbZone.SetActive(true);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            reverbZone.SetActive(false);
        }
    }
}
