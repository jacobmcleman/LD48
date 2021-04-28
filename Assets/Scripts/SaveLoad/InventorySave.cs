using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class InventorySave : MonoBehaviour
{
    private Inventory inventory;
    private PlayerController player;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
        player = GetComponent<PlayerController>();
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
            Debug.Log("Inventory Save Triggered");
            SaveDataAsync(savefile);
        }
        
    }

    private async void SaveDataAsync(string savefile)
    {
        string path = Application.persistentDataPath + "/" + savefile;
        path += ".inventory";

        string invStr = "Gold: " + inventory.gold;
        invStr += "\nIron: " + inventory.iron;
        invStr += "\nCopper: " + inventory.copper;

        if(player)
        {
            invStr += "\nFuel: " + player.DrillCurrentFuel;
        }

        using (StreamWriter writer = File.CreateText(path))
        {
            await writer.WriteAsync(invStr);
            Debug.Log("Saved to " + path);
        }
    }

    private void LoadData(string savefile)
    {
        string path = Application.persistentDataPath + "/" + savefile;
        path += ".inventory";

        List<string> lines = new List<string>();

        if(!File.Exists(path)) 
        {
            Debug.Log("No inventory save file found, treating as new game");
            return;
        }

        try
        {
            using (StreamReader reader = new StreamReader(path))
            {
                while(!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            };
        } catch(System.Exception e)
        {
            Debug.LogErrorFormat("Save File {0} could not be read!\n{1}", savefile, e.Message);
        }

        foreach(string invLine in lines)
        {
            int splitPoint = invLine.IndexOf(':');
            string tag = invLine.Substring(0, splitPoint);
            string data = invLine.Substring(splitPoint + 1);

            switch(tag)
            {
            case "Gold":
                inventory.gold = int.Parse(data);
                break;
            case "Iron":
                inventory.iron = int.Parse(data);
                break;
            case "Copper":
                inventory.copper = int.Parse(data);
                break;
            case "Fuel":
                if(player)
                {
                    player.DrillCurrentFuel = float.Parse(data);
                }
                break;
            default:
                Debug.LogError("Unrecognized inventory item?!?!");
                break;
        }
        }
                    
        
        inventory.inventoryChanged.Invoke();
    }
}
