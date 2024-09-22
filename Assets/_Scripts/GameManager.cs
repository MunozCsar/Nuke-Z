using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
    Este script controla las funciones y variables del juego que son usadas por otras partes del juego.
*/

public class GameManager : MonoBehaviour
{
    public GameObject player, pointsInstance, pauseCanvas;
    public AudioSource audioSource;
    public AudioClip[] uiSoundEffect;
    public TMP_Text hpText;
    public static GameManager Instance { get; private set; } //La instancia del GameManager, usada para acceder a los m�todos y variables de esta script desde cualquier otra script.
    public bool isPaused, instaKill;
    public GameObject[] weaponPrefabs;
    public GameObject[] DamageIndicators;
    public GameObject[] powerUpArray;
    public GameObject[] mysteryBox;
    public ParticleSystem[] bloodFX;
    public ParticleSystem wallChipFX;
    public GameObject bulletHoleDecal;
    public ParticleSystem groundFX;
    public int powerUp_max, powerUp_current;
    public float powerUpChance;
    public ParticleSystem powerUp_fx;
    [SerializeField] GameObject options, graphics, controls, volume, credits;
    public int score, kills, pointsOnHit, pointsOnKill, pointsOnHead, pointsOnNuke, killScore, playerScore;
    public bool doublePoints, gameOver, powerOn;
    public float instaKillTimer, instaKillCountdown, doublePointsTimer, doublePointsCountdown;


    #region Zombie Spawn Variables

    [Header("Zombie prefabs")]
    public GameObject[] zombie;
    public float zm_HP;

    public int zm_Damage;
    public List<GameObject> zombieList;

    [Header("Waves")]
    public int wave = 1, powerUps;
    public bool roundEnd;
    public TMP_Text waveText;
    public float zm_Delay;

    [Header("Zombie count")]
    public int zm_Count;
    public int zm_maxHorde = 24;
    public int zm_spawned = 0;
    public int zm_alive;

    [Header("Timer")]
    public int timer = 0;
    public int timerGoal = 25;
    public bool spawnZM = false;
    #endregion
    private void Awake() //Al iniciar el juego, se comprueba si ya existe una instancia del GameManager, si no la hay, este objeto se vuelve la instancia, si la hay, se destruye este objeto.
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        AssignVariables();
        InputManager.Instance.pause += PauseGame;
        pauseCanvas.SetActive(false);   
    }

    public void PlayUISound(int i)
    {
        audioSource.PlayOneShot(uiSoundEffect[i]);
    }

    void PauseGame()
    {

        if (!isPaused) //Si el juego no est� pausado y se presiona la tecla de Escape se activa el menu de pausa
        {
            UIManager.Instance.PauseMenu();
        }
        else if (isPaused) //Si el juego est� pausado y se presiona la tecla de Escape se desactiva el menu de pausa
        {

            UIManager.Instance.ResumeGame();
        }
        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void AssignVariables()
    {

        try
        {
        IncreaseZombieHP(wave);
        }
        catch { }

        try
        {
        options.SetActive(false);
        }
        catch { }

        try
        {
        graphics.SetActive(false);
        }
        catch { }

        try
        {
        controls.SetActive(false);
        }
        catch { }

        try
        {
        volume.SetActive(false);
        }
        catch { }

        try
        {
        credits.SetActive(false);
        }
        catch { }

    }

    // Update is called once per frame
    void Update()
    {
        #region PowerupTimers
        if(UIManager.Instance.instaKillUI != null)
        {
            if (instaKill && instaKillTimer < 30)
            {
                instaKillTimer += Time.deltaTime;
                instaKillCountdown -= Time.deltaTime;
                UIManager.Instance.instaKillUI.SetActive(true);
                UIManager.Instance.instaKillUI.GetComponent<TMP_Text>().text = "Instakill: " + Mathf.FloorToInt(instaKillCountdown);
            } //Timer de 30 segundos 
            else
            {
                instaKill = false;
                instaKillTimer = 0f;
                UIManager.Instance.instaKillUI.SetActive(false);
                instaKillCountdown = 30f;
            }
        }

        if(UIManager.Instance.doublePointsUI != null)
        {
            if (doublePoints && doublePointsTimer < 30)
            {
                doublePointsTimer += Time.deltaTime;
                doublePointsCountdown -= Time.deltaTime;
                UIManager.Instance.doublePointsUI.SetActive(true);
                UIManager.Instance.doublePointsUI.GetComponent<TMP_Text>().text = "Double Points: " + Mathf.FloorToInt(doublePointsCountdown);
            } //Timer de 30 segundos 
            else
            {
                doublePoints = false;
                doublePointsTimer = 0f;
                UIManager.Instance.doublePointsUI.SetActive(false);
                doublePointsCountdown = 30f;
            }
        }

        #endregion
    }

    #region PointManager
    public void AddPoints(int points)
    {
        if (!doublePoints) //Si el powerup de doble puntuacion no est� activo se suma la cantidad base de puntos
        {
            score += points;
            playerScore += points;
            pointsInstance.GetComponent<TMP_Text>().color = new Color32(255, 174, 0, 255);
            pointsInstance.GetComponent<TMP_Text>().text = points.ToString();
        }
        else //Si el powerup de doble puntuacion est� activo se suma la cantidad base de puntos multiplicado por 2
        {
            score += points * 2;
            playerScore += points * 2;
            pointsInstance.GetComponent<TMP_Text>().color = new Color32(255, 174, 0, 255);
            pointsInstance.GetComponent<TMP_Text>().text = (points * 2).ToString();
        }

        Instantiate(pointsInstance, UIManager.Instance.scoreText.transform); //Se instancia la animaci�n de los puntos
        UIManager.Instance.UpdateScoreText();
    }

    public void ReduceScore(int cost) //Se reduce la puntuaci�n del jugador por el valor del coste dado
    {
        score -= cost;
        pointsInstance.GetComponent<TMP_Text>().color = new Color32(200, 0, 0, 255);
        pointsInstance.GetComponent<TMP_Text>().text = "-" + cost.ToString();
        Instantiate(pointsInstance, UIManager.Instance.scoreText.transform);
        UIManager.Instance.UpdateScoreText();
    }
    #endregion

    public void NukePowerUp() //Funcion del powerup de bomba nuclear
    {
        UIManager.Instance.nukeUI.GetComponent<Animator>().Play("RadDepletion");
        AddPoints(pointsOnNuke);
        UIManager.Instance.UpdateScoreText();
        UIManager.Instance.UpdateScoreBoard();
        foreach (GameObject zombie in zombieList) //Recorre la lista de zombies y llama a su funci�n de muerte
        {
            zombie.GetComponent<ZM_AI>().ZM_Nuke();
            zm_alive--;
            killScore++;
        }
        zombieList.Clear(); //Limpia la lista entera 

    }

    public void InstancePowerUp(int i, Vector3 pos) //Instancia  el powerup en la posici�n y rotaci�n dada
    {
        Quaternion rot = powerUpArray[i].transform.rotation;
        Instantiate(powerUpArray[i], pos, rot);
    }

    public void ShowDamageIndicators(float hp) //Al recibir da�o, activa el contenedor de las animaciones de da�o y las ejecuta en base a la cantidad de vida del jugador
    {
        UIManager.Instance.damageIndicatorsContainer.SetActive(true);
        if (hp > 100)
        {
            int rnd = Random.Range(0, DamageIndicators.Length); //Ejecuta una de las 4 animaciones posibles
            DamageIndicators[rnd].GetComponent<Animator>().Play("Damage_Fadeout");
        }
        else
        {
            DamageIndicators[4].GetComponent<Animator>().SetTrigger("damage");
        }

        DamageIndicators[5].GetComponent<Animator>().Play("RedScreenDamage");

    }

    #region SceneLoading
    IEnumerator LoadSceneAsync(int sceneID)
    {
        UIManager.Instance.loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);
        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            UIManager.Instance.loadingBar.fillAmount = progressValue;

            yield return null;
        }
    }
    public void Play(int sceneID)
    {
        StartCoroutine(LoadSceneAsync(sceneID));
        AssignVariables();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void EndGame() //Carga la escena con la cinem�tica final
    {
        SceneManager.LoadScene(2);
    }

    public void GameOver(GameObject player) //Acaba la partida, bloquea el movimiento del jugador y de la c�mara, muestra la pantalla de puntuaci�n y, tras 15 segundos, carga el menu principal
    {
        gameOver = true;
        player.GetComponent<CameraMovement>().enabled = false;
        player.GetComponent<PlayerController>().enabled = false;
        UIManager.Instance.UpdateScoreBoard();
        UIManager.Instance.UpdateScoreText();
        UIManager.Instance.scoreBoard.SetActive(true);
        StartCoroutine(ReturnToMenu(15));
    }

    public IEnumerator ReturnToMenu(float seconds) //Vuelve al menu principal pasados los segundos indicados
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(0);
        AssignVariables();
    }
    public void Exit() //Sale del juego
    {
        Application.Quit();
    }
    #endregion
    public float IncreaseZombieHP(int wave) //Aumenta la vida de los enemigos en un valor fijo hasta la ronda 9, y tras la ronda 10 lo multiplica for 1.1
    {
        switch (wave)
        {
            case 1:
                zm_HP = 150;
                break;
            case 2:
                zm_HP = 250;
                break;
            case 3:
                zm_HP = 350;
                break;
            case 4:
                zm_HP = 450;
                break;
            case 5:
                zm_HP = 550;
                break;
            case 6:
                zm_HP = 650;
                break;
            case 7:
                zm_HP = 750;
                break;
            case 8:
                zm_HP = 850;
                break;
            case 9:
                zm_HP = 950;
                break;
            default:
                zm_HP *= 1.1f;
                break;
        }
        return zm_HP;
    }

}
