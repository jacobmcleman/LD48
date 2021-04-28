using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class SaveStateManager : MonoBehaviour
{
    public UnityEvent<string, SaveType> onSaveTriggered;
    public UnityEvent<string> onLoadTriggered;

    public string saveFileName = "test_save";

    public enum SaveType
    {
        Everything, Player, WorldState
    }

    private void Update()
    {
        if(Keyboard.current[Key.F2].wasPressedThisFrame)
        {
            TriggerSave();
        }
        if(Keyboard.current[Key.F3].wasPressedThisFrame)
        {
            TriggerLoad();
        }
    }

    public void TriggerSave()
    {
        onSaveTriggered.Invoke(saveFileName, SaveType.Everything);
    }

    public void TriggerLoad()
    {
        onLoadTriggered.Invoke(saveFileName);
    }
}
