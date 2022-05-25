using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// There's probably a better way of doing this: https://gamedev.stackexchange.com/questions/110958/what-is-the-proper-way-to-handle-data-between-scenes
// See Inversion of Control / Dependency Injection frameworks
// HOWEVER -- maybe that's overkill for this project

public class SaveSystem 
{
    // returns the current SaveProfile being used
    public static SaveProfile Current {
        get {
            if (current == null && SGrid.current != null) // SGrid.current != null meas we are in play
            {
                Debug.LogError("Save System is not using a profile! Creating a default profile for now...");
                current = new SaveProfile("Boomo");
                currentIndex = -1;
            }
            return current;
        }
        private set {
            current = value;
        }
    }
    private static SaveProfile current;
    private static int currentIndex; // if -1, then it's a temporary profile

    private static SaveProfile[] saveProfiles = new SaveProfile[3];

    public SaveSystem() {
        // load profiles from file here maybe
        saveProfiles[0] = new SaveProfile("Temp Catto");
    }

    public static SaveProfile GetProfile(int index)
    {
        return saveProfiles[index];
    }

    public static void SetProfile(int index, SaveProfile profile)
    {
        saveProfiles[index] = profile;
    }

    public static void SetCurrentProfile(int index)
    {
        Current = GetProfile(index);
    }


    // saves the current game to file
    public static void SaveGame()
    {
        current.Save();

        SerializableSaveProfile profile = SerializableSaveProfile.FromSaveProfile(current);

        currentIndex = 1;
        SaveToFile(profile, currentIndex);
    }

    private static void SaveToFile(SerializableSaveProfile profile, int index)
    {
        Debug.Log("Saving data to file...");

        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + string.Format("/slider{0}.cat", index);
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, profile);

        bool doJson = false;
        if (doJson)
        {
            string json = JsonUtility.ToJson(profile);
            Debug.Log(json);

            StreamWriter sr = new StreamWriter(stream);
            sr.WriteLine(json);
            sr.Close();
        }

        stream.Close();
    }
}
