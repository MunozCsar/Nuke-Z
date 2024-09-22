using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_UX : MonoBehaviour
{ 
    [SerializeField] GameObject options;
    [SerializeField] GameObject graphics;
    [SerializeField] GameObject controls;
    [SerializeField] GameObject volume;
    [SerializeField] GameObject credits;
    public Image loadingBar;
    public GameObject loadingScreen;
    private void Start()
    {
        Cursor.visible = true; //Activa la visibilidad del cursor
        Cursor.lockState = CursorLockMode.None; //Habilita el cursor
        options.SetActive(false);
        graphics.SetActive(false);
        controls.SetActive(false);
        volume.SetActive(false);
        credits.SetActive(false);
    }
    public void Play(int sceneID)
    {
        StartCoroutine(LoadSceneAsync(sceneID));
    }

    IEnumerator LoadSceneAsync(int sceneID)
    {
        loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);
        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            loadingBar.fillAmount = progressValue;

            yield return null;
        }
    }
    public void Options()
    {
        options.SetActive(true);
    }
    public void Exit()
    {
        Application.Quit();
    }
    public void BackOptions()
    {
        options.SetActive(false);
    }
    public void BackGraphisAndVolumeAndControlsAndCredits()
    {
        graphics.SetActive(false);
        volume.SetActive(false);
        controls.SetActive(false);
        credits.SetActive(false);
    }
    
    public void Credits()
    {
        credits.SetActive(true);
    }
    public void Graphics()
    {
        graphics.SetActive(true);
    }
    public void Volume()
    {
        volume.SetActive(true);
    }
    public void Controls()
    {
       controls.SetActive(true);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
