using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FX
{
    Fire = 0,
    Reload = 1,
    EmptyReload = 2,
    Movement = 3
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }


    [SerializeField] AudioClip[] soundEffects;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void PlayFX(AudioSource audioSource, FX fx)
    {
        audioSource.PlayOneShot(soundEffects[(int)fx]);
        Debug.Log("Bang!");
    }

    public void PlayOnEvent()
    {
        Debug.Log("Play");
    }
}
