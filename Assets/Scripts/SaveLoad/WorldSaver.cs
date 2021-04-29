using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;
using UnityEngine;

[RequireComponent(typeof(WorldBuilder))]
public class WorldSaver : MonoBehaviour
{
    private WorldBuilder world;

    private void Awake()
    {
        world = GetComponent<WorldBuilder>();
    }

    private void Start()
    {
        SaveStateManager saveMan = FindObjectOfType<SaveStateManager>();
        saveMan.onSaveTriggered.AddListener(SaveData);
        saveMan.onLoadTriggered.AddListener(LoadData);
    }

    private void SaveData(string savefile, SaveStateManager.SaveType type)
    {
        if(type != SaveStateManager.SaveType.Player)
        {
            Debug.Log("World Save Triggered");
            SaveDataAsync(savefile);
        }
    }

    private void LoadData(string savefile)
    {
        string path = Application.persistentDataPath + "/" + savefile;
        path += ".world";

        if(!File.Exists(path)) 
        {
            Debug.Log("No world save file found, treating as new game");
            return;
        }

        try
        {
            string seedStr;
            string boundsStr;
            string encodedWorldStr;
            using (StreamReader reader = new StreamReader(path))
            {
                seedStr = reader.ReadLine();
                boundsStr = reader.ReadLine();
                encodedWorldStr = reader.ReadLine();
            };

            world.seed = int.Parse(seedStr);
            int minBound = int.Parse(boundsStr.Substring(0, boundsStr.IndexOf(",")));
            int maxBound = int.Parse(boundsStr.Substring(boundsStr.IndexOf(",") + 1));

            

        } catch(System.Exception e)
        {
            Debug.LogErrorFormat("Save File {0} could not be read!\n{1}", savefile, e.Message);
        }
    }

    private string stringify(TileBase tile)
    {
        if(tile == null) return "0";
        else if(tile == world.sandTile) return "1";
        else if(tile == world.rockTile) return "2";
        else if(tile == world.goldTile) return "3";
        else if(tile == world.ironTile) return "4";
        else if(tile == world.copperTile) return "5";
        else return "0";
    }

    private async void SaveDataAsync(string savefile)
    {
        string path = Application.persistentDataPath + "/" + savefile;
        path += ".world";

        string serialized = "";
        serialized = world.seed.ToString();
        serialized += "\n" + world.MinBound + "," + world.MaxBound;
        TileBase[] tiles = world.GetTilesRaw();

        System.Action serializeTilesAction = () =>
        {
            serialized += "\n";

            int runLengthCount = 1;
            string compressed = "";
            for(int i = 0; i < tiles.Length; ++i)
            {
                if(i < tiles.Length - 1 && tiles[i] == tiles[i+1])
                {
                    runLengthCount++;
                }
                else 
                {
                    compressed += stringify(tiles[i]);
                    compressed += "-" + runLengthCount + ",";
                    runLengthCount = 1;
                }
            }
            serialized += compressed;
        };


        using (StreamWriter writer = File.CreateText(path))
        {
            Task serializeTask = new Task(serializeTilesAction);
            serializeTask.Start();
            await serializeTask;
            await writer.WriteAsync(serialized);
            Debug.Log("Saved to " + path);
        }
    }
}
