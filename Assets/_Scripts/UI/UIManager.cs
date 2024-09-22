using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance { get; private set; }

    public GameObject options, graphics, controls, volume, credits, loadingScreen, maxAmmoUI, nukeUI, instaKillUI, doublePointsUI, pauseFirstButton, optionsFirstButton, controlsFirstButton, graphicsFirstButton, volumeFirstButton;
    public Image loadingBar;
    public GameObject scoreBoard, pauseCanvas, damageIndicatorsContainer;
    public TMP_Text scoreText, totalScore, totalKills, interactText;
    public Slider volumeSlider, sensitivitySlider;
    private float slidervalue = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InputManager.Instance._inputActions.Player.Scoreboard.started += ctx => ShowScoreBoard();
        InputManager.Instance._inputActions.Player.Scoreboard.canceled += ctx => HideScoreBoard();
        UpdateScoreText();
    }

    #region UI

    private void HideScoreBoard()
    {
        scoreBoard.SetActive(false);
    }

    private void ShowScoreBoard()
    {
        if (!GameManager.Instance.isPaused)
        {
            scoreBoard.SetActive(true);
        }
        else if (GameManager.Instance.isPaused)
        {
            scoreBoard.SetActive(false);
        }

    }

    public void UpdateScoreText() //Actualiza el texto de la puntuacion en pantalla
    {
        scoreText.text = GameManager.Instance.score.ToString();
    }

    public void UpdateScoreBoard() //Actualiza los textos de puntuacion y bajas de la pantalla de puntuacion
    {
        totalScore.text = GameManager.Instance.playerScore.ToString();
        totalKills.text = GameManager.Instance.killScore.ToString();
    }
    public void Options() //Muestra la pantalla de opciones
    {
        options.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
    }

    public void BackOptions() //Vuelve al menu de pausa
    {
        options.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(pauseFirstButton);
    }
    public void BackGraphisAndVolumeAndControlsAndCredits() //Vuelve al menu de opciones
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
        graphics.SetActive(false);
        volume.SetActive(false);
        controls.SetActive(false);
        if (credits != null)
        {
            credits.SetActive(false);
        }


    }

    public void Credits() //Muestra la pantalla de creditos
    {
        credits.SetActive(true);

    }
    public void Graphics() //Muestra la pantalla de graficos
    {
        graphics.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(graphicsFirstButton);
    }
    public void Volume() //Muestra la pantalla de volumen
    {
        volume.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(volumeFirstButton);
    }
    public void Controls() //Muestra la pantalla de controles
    {
        controls.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(controlsFirstButton);
    }
    public void MainMenu() //Vuelve al menu principal
    {
        SceneManager.LoadScene(0);
    }

    public void ChangeSlider(float valor) //Cambia el valor del slider del volumen
    {
        volumeSlider.value = valor;
        PlayerPrefs.SetFloat("volumenAudio", slidervalue);
        AudioListener.volume = slidervalue;
    }

    public void PauseMenu() //Pausa la partida
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(pauseFirstButton);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        GameManager.Instance.isPaused = true;
        GameManager.Instance.pauseCanvas.SetActive(true);
        HideScoreBoard();
    }

    public void ResumeGame() //Continua la partida
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GameManager.Instance.isPaused = false;
        GameManager.Instance.pauseCanvas.SetActive(false);
        Time.timeScale = 1;

    }
    #endregion
}
