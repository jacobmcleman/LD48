using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SaveStateManager : MonoBehaviour
{
    public UnityEvent<string, SaveType, SaveStateManager> onSaveTriggered;
    public UnityEvent<string> onLoadTriggered;

    public string saveFileName = "test_save";

    private GameObject saveIndicator;

    public enum SaveType
    {
        Everything, Player, WorldState
    }

    private int savesActive = 0;
    private bool shutdownRequested = false;

    private void Start()
    {
        saveIndicator = GameObject.Find("UI_Canvas").transform.Find("Save Indicator").gameObject;
        saveIndicator.SetActive(false);
        TriggerLoad();
    }

    private void Update()
    {
        if(shutdownRequested && savesActive <= 0)
        {
            Debug.Log("All saves completed, shutting down");
            Application.Quit();
            shutdownRequested = false; // for editor that ignores quit
        }
    }

    public void TriggerSave(SaveType type = SaveType.Everything)
    {
        onSaveTriggered.Invoke(saveFileName, type, this);
    }

    public void TriggerLoad()
    {
        onLoadTriggered.Invoke(saveFileName);
    }

    public void RequestGameExit()
    {
        shutdownRequested = true;
        Debug.Log("EXIT REQUESTED");
    }

    public void RegisterSaveProcessStarted()
    {
        if(savesActive == 0)
        {
            saveIndicator.SetActive(true);
        }

        savesActive++;
    }

    public void RegisterSaveProcessCompleted()
    {
        savesActive--;

        if(savesActive == 0)
        {
            saveIndicator.SetActive(false);
        }
    }
}
