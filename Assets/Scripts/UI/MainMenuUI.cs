using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Text playButtonText;

    private CameraFollow cameraFollow;
    private Transform player;

    public GameObject gameUI;

    public bool paused;

    public bool hasStarted;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        gameUI.SetActive(false);
        Time.timeScale = 0;
        paused = true;
        hasStarted = false;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        gameObject.SetActive(true);
        paused = true;
        FindObjectOfType<SaveStateManager>().TriggerSave();
    }

    public void StartResumeGame()
    {
        cameraFollow.target = player;
        Time.timeScale = 1;

        playButtonText.text = "Resume";

        gameObject.SetActive(false);
        gameUI.SetActive(true);

        paused = false;
        hasStarted = true;
    }

    public void ShowOptions()
    {
        // TODO: options
        Debug.Log("OPTIONS");
    }

    public void ExitGame()
    {
        // TODO: Confirmation Prompt
        Debug.Log("QUIT");
        Application.Quit();
    }
}
