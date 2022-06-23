using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    private static int currentIndex = -1; // if -1, then it's a temporary profile

    private static SaveProfile[] saveProfiles = new SaveProfile[3];

    public SaveSystem()
    {
        SetProfile(0, GetSerializableSaveProfile(0)?.ToSaveProfile());
        SetProfile(1, GetSerializableSaveProfile(1)?.ToSaveProfile());
        SetProfile(2, GetSerializableSaveProfile(2)?.ToSaveProfile());
    }

    public static SaveProfile GetProfile(int index)
    {
        if (index == -1)
            return null;
        return saveProfiles[index];
    }

    public static int GetRecentlyPlayedIndex()
    {
        int ret = -1;
        System.DateTime mostRecent = System.DateTime.MinValue;
        for (int i = 0; i < 3; i++)
        {
            if (saveProfiles[i] != null && saveProfiles[i].GetLastSaved() > mostRecent)
            {
                ret = i;
                mostRecent = saveProfiles[i].GetLastSaved();
            }
        }
        return ret;
    }

    public static void SetProfile(int index, SaveProfile profile)
    {
        saveProfiles[index] = profile;
    }

    public static void SetCurrentProfile(int index)
    {
        currentIndex = index;
        Current = GetProfile(index);
    }


    /// <summary>
    /// Saves the game to the current loaded profile index (either 1, 2, or 3). If the profile index is -1, then no data will be saved.
    /// </summary>
    public static void SaveGame()
    {
        if (currentIndex == -1)
            return;

        current.Save();

        SerializableSaveProfile profile = SerializableSaveProfile.FromSaveProfile(current);

        SaveToFile(profile, currentIndex);
    }

    private static void SaveToFile(SerializableSaveProfile profile, int index)
    {
        Debug.Log("Saving data to file...");

        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetFilePath(index);
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, profile);

        // in case we need json somewhere in the future? idk
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

    public static void LoadSaveProfile(int index)
    {
        SerializableSaveProfile ssp = null;

        ssp = GetSerializableSaveProfile(index);

        SaveProfile profile;
        if (ssp == null)
        {
            Debug.LogError("Creating a new temporary save profile -- this shouldn't happen!");
            profile = new SaveProfile("Boomo");
        }
        else
        {
            profile = ssp.ToSaveProfile();
        }

        current = profile;
        currentIndex = index;


        // This makes it so the profile gets loaded first thing in the new scene
        SceneInitializer.profileToLoad = current;

        // Load last scene the player was in
        SceneManager.LoadScene(current.GetLastArea().ToString());
    }

    public static SerializableSaveProfile GetSerializableSaveProfile(int index)
    {
        return LoadFromFile(index);
    }

    private static SerializableSaveProfile LoadFromFile(int index)
    {
        Debug.Log("Loading data from file " + index + "...");

        string path = GetFilePath(index);
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SerializableSaveProfile profile = formatter.Deserialize(stream) as SerializableSaveProfile;
            stream.Close();

            return profile;
        }
        else
        {
            Debug.LogWarning("Save file not found at " + path);
            return null;
        }
    }

    public static void DeleteSaveProfile(int index)
    {
        Debug.Log("Deleting Save profile #" + index + "!");

        string path = GetFilePath(index);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static string GetFilePath(int index)
    {
        return Application.persistentDataPath + string.Format("/slider{0}.cat", index);
    }
}
