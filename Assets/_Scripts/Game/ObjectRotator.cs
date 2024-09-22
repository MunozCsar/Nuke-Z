using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{

    public Vector3 rotDir;
    public float rotSpeed, lifeTime;
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Rotación del objeto en la dirección y a la velocidad especificadas
    void Update()
    {
        transform.Rotate(rotDir * rotSpeed * Time.deltaTime);
    }
}
