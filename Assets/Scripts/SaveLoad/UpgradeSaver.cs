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
        SaveStateManager saveMan = FindObjectOfType<SaveStateManager>();
        saveMan.onSaveTriggered.AddListener(SaveData);
        saveMan.onLoadTriggered.AddListener(LoadData);
    }

    public void SaveData(string savefile, SaveStateManager.SaveType type)
    {
        if(type != SaveStateManager.SaveType.WorldState)
        {
            Debug.Log("Upgrades Save Triggered");
            SaveDataAsync(savefile);
        }
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

    private void LoadData(string savefile)
    {
        string path = Application.persistentDataPath + "/" + savefile;
        path += ".upgrades";

        if(!File.Exists(path)) 
        {
            Debug.Log("No upgrade save file found, treating as new game");
            return;
        }

        try
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string upgradesString = reader.ReadToEnd();
                upgradeManager.DeserializeUpgrades(upgradesString);
            };
        } catch(System.Exception e)
        {
            Debug.LogErrorFormat("Save File {0} could not be read!\n{1}", savefile, e.Message);
        }
        
    }
}
