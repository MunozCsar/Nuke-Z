using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentCollider : MonoBehaviour
{

    public GameObject bone;

    // Actualiza la posición del GO a la posición del hueso indicado
    void Update()
    {
        gameObject.transform.rotation = bone.transform.rotation;
        gameObject.transform.position = bone.transform.position;
    }
}
