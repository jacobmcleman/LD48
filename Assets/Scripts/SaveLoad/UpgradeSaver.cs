using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(UpgradeManager))]
public class UpgradeSaver : MonoBehaviour
{
    private UpgradeManager upgradeManager;

    private void Awake()
    {
        upgradeManager = GetComponent<UpgradeManager>();
    }

    private void Start()
    {
        FindObjectOfType<SaveStateManager>().onSaveTriggered.AddListener(SaveData);
    }

    public void SaveData(string savefile)
    {
        Debug.Log("Save Triggered");
        SaveDataAsync(savefile);
    }

    private async void SaveDataAsync(string savefile)
    {
        string path = Application.persistentDataPath + "/" + savefile;
        path += ".upgrades";
        using (StreamWriter writer = File.CreateText(path))
        {
            await writer.WriteAsync(upgradeManager.SerializeUpgrades());
            Debug.Log("Saved to " + path);
        }
    }
}
