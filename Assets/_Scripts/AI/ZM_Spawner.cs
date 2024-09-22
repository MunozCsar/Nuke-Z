using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

/*
    Este script gestiona la aparición de zombies durante el juego y controla varios parámetros del juego
    como el recuento de oleadas, el recuento de zombies, los tiempos de aparición y las transiciones de rondas.
*/

public class ZM_Spawner : MonoBehaviour
{
    [Header("Generadores")]
    public Transform[] zmSpawners;
    public bool activeSpawner;

    void Start()
    {
        NextWave();
        GameManager.Instance.waveText.text = GameManager.Instance.wave.ToString();
        GameManager.Instance.IncreaseZombieHP(GameManager.Instance.wave);
        Timings();
    }

    private void Update()
    {
        if (GameManager.Instance.timer >= GameManager.Instance.timerGoal && GameManager.Instance.roundEnd == false && activeSpawner)
        {
            ZM_Spawn();
        }
        else if (GameManager.Instance.timer < GameManager.Instance.timerGoal)
        {
            GameManager.Instance.timer++;
        }

        if (GameManager.Instance.zm_alive.Equals(0) && GameManager.Instance.zm_spawned.Equals(GameManager.Instance.zm_Count))
        {
            GameManager.Instance.roundEnd = true;
            GameManager.Instance.waveText.GetComponent<Animator>().SetTrigger("onRoundChange");
            StartCoroutine(NewRound());
        }
    }

    // Determina la cantidad de zombies para la próxima oleada
    private void NextWave()
    {
        switch (GameManager.Instance.wave)
        {
            default:
                GameManager.Instance.zm_Count = Mathf.RoundToInt(0.0842f * Mathf.Pow(GameManager.Instance.wave, 2) + 0.1954f * GameManager.Instance.wave + 22.05f);
                break;
            case 1:
                GameManager.Instance.zm_Count = 6;
                break;
            case 2:
                GameManager.Instance.zm_Count = 9;
                break;
            case 3:
                GameManager.Instance.zm_Count = 13;
                break;
            case 4:
                GameManager.Instance.zm_Count = 15;
                break;
            case 5:
                GameManager.Instance.zm_Count = 18;
                break;
            case 6:
                GameManager.Instance.zm_Count = 21;
                break;
            case 7:
                GameManager.Instance.zm_Count = 24;
                break;
            case 8:
                GameManager.Instance.zm_Count = 27;
                break;
            case 9:
                GameManager.Instance.zm_Count = 30;
                break;
            case 10:
                GameManager.Instance.zm_Count = 32;
                break;
            case 11:
                GameManager.Instance.zm_Count = 35;
                break;
        }

        GameManager.Instance.powerUp_current = 0;
    }

    // Configura los tiempos para la aparición de zombies según la oleada actual
    private void Timings()
    {
        switch (GameManager.Instance.wave)
        {
            case 1:
                GameManager.Instance.zm_Delay = 10f;
                GameManager.Instance.timerGoal = 500;
                break;
            case 5:
                GameManager.Instance.zm_Delay = 7.5f;
                GameManager.Instance.timerGoal = 400;
                break;
            case 10:
                GameManager.Instance.zm_Delay = 5f;
                GameManager.Instance.timerGoal = 325;
                break;
            case 20:
                GameManager.Instance.zm_Delay = 3.5f;
                GameManager.Instance.timerGoal = 200;
                break;
        }
    }

    // Función que gestiona la aparición de zombies
    private void ZM_Spawn()
    {
        if (GameManager.Instance.zm_alive < GameManager.Instance.zm_maxHorde && GameManager.Instance.roundEnd == false && GameManager.Instance.isPaused == false)
        {
            if (GameManager.Instance.zm_spawned < GameManager.Instance.zm_Count)
            {
                GameManager.Instance.zm_spawned++;
                GameManager.Instance.zm_alive++;
                int rnd = Random.Range(0, zmSpawners.Length);
                if (zmSpawners[rnd].CompareTag("BarrierSpawner"))
                {
                    GameManager.Instance.zombieList.Add(Instantiate(GameManager.Instance.zombie[0], zmSpawners[rnd].transform.position, Quaternion.identity));
                    Instantiate(GameManager.Instance.groundFX, zmSpawners[rnd].transform.position, Quaternion.identity);
                }
                else if (zmSpawners[rnd].CompareTag("NormalSpawner"))
                {
                    GameManager.Instance.zombieList.Add(Instantiate(GameManager.Instance.zombie[1], zmSpawners[rnd].transform.position, Quaternion.identity));
                    Instantiate(GameManager.Instance.groundFX, zmSpawners[rnd].transform.position, Quaternion.identity);
                }
                GameManager.Instance.timer = 0;
            }
        }
    }

    // Corrutina para iniciar una nueva ronda
    private IEnumerator NewRound()
    {
        GameManager.Instance.zm_spawned = 0;
        yield return new WaitForSeconds(GameManager.Instance.zm_Delay);
        GameManager.Instance.wave++;
        GameManager.Instance.waveText.GetComponent<Animator>().SetTrigger("onRoundStart");
        if (GameManager.Instance.wave < 55)
        {
            GameManager.Instance.zombie[0].GetComponent<ZM_AI>().hp = GameManager.Instance.IncreaseZombieHP(GameManager.Instance.wave);
        }
        NextWave();
        Timings();
        GameManager.Instance.waveText.text = GameManager.Instance.wave.ToString();
        GameManager.Instance.timer = 0;
        GameManager.Instance.roundEnd = false;
        for (int i = 0; i < GameManager.Instance.mysteryBox.Length; i++)
        {
            GameManager.Instance.mysteryBox[i].GetComponent<MysteryBox>().currentUses = 0;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeSpawner = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeSpawner = false;
        }
    }
}



