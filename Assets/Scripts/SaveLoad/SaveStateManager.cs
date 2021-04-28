using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SaveStateManager : MonoBehaviour
{
    public UnityEvent<string, SaveType> onSaveTriggered;
    public UnityEvent<string> onLoadTriggered;

    public string saveFileName = "test_save";

    public enum SaveType
    {
        Everything, Player, WorldState
    }

    private void Start()
    {
        TriggerLoad();
    }

    public void TriggerSave(SaveType type = SaveType.Everything)
    {
        onSaveTriggered.Invoke(saveFileName, type);
    }

    public void TriggerLoad()
    {
        onLoadTriggered.Invoke(saveFileName);
    }
}
