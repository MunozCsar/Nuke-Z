using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StoreVariables : MonoBehaviour
{

    public Slider sensitivitySlider;
    public float sensitivity;

    void Start() {
        sensitivity = PlayerPrefs.GetFloat("sensitivity", .5f);
    }

    public void UpdateSensitivty()
    {
        sensitivity = sensitivitySlider.value;
        PlayerPrefs.SetFloat("sensitivity", sensitivity);
    }
}
