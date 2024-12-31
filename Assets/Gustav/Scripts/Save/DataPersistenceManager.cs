using UnityEngine;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager Instance { get; private set; }

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    private GameData gameData;
    private FileDataHandler dataHandler;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
    }

    public void NewGame()
    {
        gameData = new GameData();
    }

    public void LoadSpecifiedData(IDataPersistence dataPersistenceObject)
    {
        if (gameData == null)
        {
            NewGame();
            return;
        }

        dataPersistenceObject.LoadData(gameData);
    }

    public void LoadData()
    {
        gameData = dataHandler.Load();

        if (gameData == null)
        {
            NewGame();
            return;
        }

        foreach (IDataPersistence dataPersistenceObject in FindAllDataPersistenceObjects())
        {
            dataPersistenceObject.LoadData(gameData);
        }
    }

    public void SaveSpecifiedData(IDataPersistence dataPersistenceObject)
    {
        if (gameData == null)
        {
            NewGame();
        }

        dataPersistenceObject.SaveData(gameData);
        dataHandler.Save(gameData);
    }

    public void SaveData()
    {
        if (gameData == null)
        {
            NewGame();
        }

        foreach (IDataPersistence dataPersistenceObject in FindAllDataPersistenceObjects())
        {
            dataPersistenceObject.SaveData(gameData);
        }

        dataHandler.Save(gameData);
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType(typeof(MonoBehaviour), FindObjectsSortMode.None).OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }
}
