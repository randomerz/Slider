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
            if (current == null && SGrid.Current != null) // SGrid.current != null meas we are in play
            {
                Debug.LogWarning("[File IO] Save System is not using a profile! Creating a default profile for now...");
                current = new SaveProfile("Boomo");
                currentIndex = -1;
            }
            return current;
        }
        private set {
            current = value;
        }
    }

    public static DeserializationTypeRemapBinder AssemblyRemapBinder
    {
        get
        {
            if (_assemblyRemapBinder == null)
            {
                _assemblyRemapBinder = new DeserializationTypeRemapBinder();
                _assemblyRemapBinder.AddAssemblyMapping("Assembly-CSharp", "SliderScripts");
            }

            return _assemblyRemapBinder;
        }
    }
    private static DeserializationTypeRemapBinder _assemblyRemapBinder;

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

    public static bool IsCurrentProfileNull()
    {
        return Current == null;
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
    /// Saves the game to the current loaded profile index (either 0, 1, or 2). If the profile index is -1, then no data will be saved.
    /// </summary>
    public static void SaveGame(string reason="")
    {
        if (currentIndex == -1)
            return;

        if (reason != "") Debug.Log($"[Saves] Saving game: {reason}");

        Current.Save();
        SetProfile(currentIndex, Current);

        SerializableSaveProfile profile = SerializableSaveProfile.FromSaveProfile(Current);

        SaveToFile(profile, currentIndex);
    }

    private static void SaveToFile(SerializableSaveProfile profile, int index)
    {
        // Debug.Log($"[File IO] Saving data to file {index}.");

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Binder = AssemblyRemapBinder;

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
        SaveSystem.SetCurrentProfile(index);
        if (Current == null)
        {
            Debug.LogError("Creating a new temporary save profile -- this shouldn't happen!");
            Current = new SaveProfile("Boomo");
        }
        
        // This makes it so the profile gets loaded first thing in the new scene
        SceneInitializer.profileToLoad = Current;

        // Load last scene the player was in
        string sceneToLoad = Current.GetLastArea().ToString();
        
        // early access
        // if (Current.GetBool("isDemoBuild") && sceneToLoad == "Caves")
        //     sceneToLoad = "Demo Caves";
        
        // early access
        // if (Current.GetBool("isDemoBuild") && sceneToLoad == "Military")
        //     sceneToLoad = "Demo Military";

        SceneManager.LoadScene(sceneToLoad);
    }

    public static SerializableSaveProfile GetSerializableSaveProfile(int index)
    {
        return LoadFromFile(index);
    }

    private static SerializableSaveProfile LoadFromFile(int index)
    {
        // Debug.Log($"[File IO] Loading data from file {index}.");

        string path = GetFilePath(index);
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = AssemblyRemapBinder;

            FileStream stream = new FileStream(path, FileMode.Open);

            SerializableSaveProfile profile = formatter.Deserialize(stream) as SerializableSaveProfile;
            stream.Close();

            return profile;
        }
        else
        {
            // Debug.LogWarning($"[File IO] Save file not found at {path}");
            return null;
        }
    }

    public static void DeleteSaveProfile(int index)
    {
        Debug.Log($"[File IO] Deleting Save profile #{index}!");

        saveProfiles[index] = null;

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
