using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Text playButtonText;

    private Text versionText;

    private CameraFollow cameraFollow;
    private Transform player;

    public GameObject gameUI;

    public bool paused;

    public bool hasStarted;

    private void Awake()
    {
        Time.timeScale = 0;
        paused = true;
        hasStarted = false;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        player.GetComponent<Rigidbody2D>().simulated = false;
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        versionText = transform.Find("Version").GetComponent<Text>();
        gameUI.SetActive(false);

        versionText.text = "Version " + Application.version + (Application.isEditor ? " [Editor]" : "");
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
        
        paused = false;
        gameObject.SetActive(false);

        if(hasStarted)
        {
            gameUI.SetActive(true);
            Time.timeScale = 1;
        }
        else
        {
            cameraFollow.inStartPan = true;
        }

        playButtonText.text = "Resume";
    }

    public void FinishStartup()
    {
       
        gameUI.SetActive(true);

        paused = false;
        hasStarted = true;

        player.GetComponent<Rigidbody2D>().simulated = true;

        Time.timeScale = 1;
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
        FindObjectOfType<SaveStateManager>().RequestGameExit();
        //Application.Quit();
    }
}
