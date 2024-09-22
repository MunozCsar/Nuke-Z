using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDowned : MonoBehaviour
{
    PlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<PlayerController>(); 
    }

    private void Update()
    {
        if (player.isDowned && Vector3.Distance(player.playerCam.transform.position, player.cameraDown_Position.position) > 0.01f)
        {
            Downed();
        }
        if (!player.isDowned && Vector3.Distance(player.playerCam.transform.position, player.cameraUp_Position.position) > 0.01f)
        {
            Stand();
        }
    }
    void Downed()
    {
        player.playerCam.transform.position = Vector3.Lerp(player.playerCam.transform.position, player.cameraDown_Position.position, player.downSpeed * Time.deltaTime);
    }
    void Stand()
    {
            player.playerCam.transform.position = Vector3.Lerp(player.playerCam.transform.position, player.cameraUp_Position.position, player.downSpeed * Time.deltaTime);
    }
}
