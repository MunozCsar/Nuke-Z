using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Volumen : MonoBehaviour
{
    public Slider slider;
    public float slidervalue;

    private void Start()
    {
        slider.value = PlayerPrefs.GetFloat("volumenAudio", 0.5f);
        AudioListener.volume = slidervalue;
    }
    public void ChangeSlider(float valor)
    {
        slider.value = valor;
        PlayerPrefs.SetFloat("volumenAudio", slidervalue);
        AudioListener.volume = slidervalue;
    }
}
