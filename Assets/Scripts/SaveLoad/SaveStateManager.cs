using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class SaveStateManager : MonoBehaviour
{
    public UnityEvent<string> onSaveTriggered;

    public string saveFileName = "test_save";

    private void Update()
    {
        if(Keyboard.current[Key.F2].wasPressedThisFrame)
        {
            TriggerSave();
        }
    }

    public void TriggerSave()
    {
        onSaveTriggered.Invoke(saveFileName);
    }
}
