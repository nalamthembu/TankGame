using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    static readonly string path = Application.persistentDataPath + "/tanks.save";

    public static bool TrySave(SaveData saveData)
    {
        try
        {
            FileStream stream = new(path, FileMode.Create);

            BinaryFormatter formatter = new();

            formatter.Serialize(stream, saveData);

            stream.Close();

            Debug.Log("Saved Successfully");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);

            return false;
        }
    }

    public static bool TryLoad(out SaveData saveData)
    {
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new();

            FileStream stream = new(path, FileMode.Open);

            saveData = (SaveData)formatter.Deserialize(stream);

            stream.Close();

            return true;
        }

        saveData = null;

        Debug.LogError("Game failed to load save game");

        return false;
    }
}