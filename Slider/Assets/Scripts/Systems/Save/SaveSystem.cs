using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

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

    // Roughly every ~3600 seconds, create a permanent backup
    private const float TIME_BETWEEN_PERMANENT_BACKUPS_SECONDS = 3600;

    private static SaveProfile current;
    private static int currentIndex = -1; // if -1, then it's a temporary profile

    private static SaveProfile[] saveProfiles = new SaveProfile[3];

    private const string GDK_GAME_SAVE_CONTAINER_NAME = "slider_container";

    private enum SaveSystemType
    {
        STEAM,
        GDK,
    }
    private const SaveSystemType SAVE_SYSTEM_TYPE = (
        #if MICROSOFT_GDK_SUPPORT
        SaveSystemType.GDK
        #else
        SaveSystemType.STEAM
        #endif
    );
    
    public static EventHandler OnGameSaveLoaded;

    public SaveSystem()
    {
        if (SAVE_SYSTEM_TYPE == SaveSystemType.GDK)
        {
            // Saves are async...
            PollForSaves();
            GDKProxy.OnGameSaveLoaded += OnGDKSavesReady;
        }
        else
        {
            SetProfile(0, GetSerializableSaveProfile(0)?.ToSaveProfile());
            SetProfile(1, GetSerializableSaveProfile(1)?.ToSaveProfile());
            SetProfile(2, GetSerializableSaveProfile(2)?.ToSaveProfile());

            if (Application.platform == RuntimePlatform.LinuxPlayer)
            {
                CreateLinuxSpecificBackups(0);
                CreateLinuxSpecificBackups(1);
                CreateLinuxSpecificBackups(2);
            }
        }
    }

    private async void PollForSaves()
    {
        Debug.Log("[Saves] Polling for saves...");
        while (!AreSavesReady())
        {
            await System.Threading.Tasks.Task.Yield();
        }
        Debug.Log("[Saves] Saves are ready!");

        SetProfile(0, GetSerializableSaveProfile(0)?.ToSaveProfile());
        SetProfile(1, GetSerializableSaveProfile(1)?.ToSaveProfile());
        SetProfile(2, GetSerializableSaveProfile(2)?.ToSaveProfile());
    }

    private void OnGDKSavesReady(object sender, System.EventArgs e)
    {
        OnGameSaveLoaded?.Invoke(this, e);
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
        Debug.Log($"[Saves] Setting current profile to {index}");
        currentIndex = index;
        Current = GetProfile(index);
    }

    public static bool AreSavesReady()
    {
        if (SAVE_SYSTEM_TYPE == SaveSystemType.GDK)
        {
            return GDKProxy.AreSavesReady();
        }
        return true;
    }


    /// <summary>
    /// Saves the game to the current loaded profile index (either 0, 1, or 2). If the profile index is -1, then no data will be saved.
    /// </summary>
    public static void SaveGame(string reason="")
    {
        if (currentIndex == -1)
            return;

        if (reason != "") Debug.Log($"[Saves] [{System.DateTime.Now}] Saving game: {reason}");

        Current.Save();
        SetProfile(currentIndex, Current);

        SerializableSaveProfile profile = SerializableSaveProfile.FromSaveProfile(Current);

        try 
        {
            SaveToFile(profile, GetFilePath(currentIndex), GetFilePathTemp(currentIndex));
            Current.SetMovedToPermanent(false);

            float timeSinceBackup = Current.GetPlayTimeInSeconds() - Current.GetLastPermanentBackupTimeInSeconds();
            if (timeSinceBackup > TIME_BETWEEN_PERMANENT_BACKUPS_SECONDS)
            {
                MoveSaveToPermanentBackup(currentIndex);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Saves] Error when saving profile. {e.Message}. Moving most recent " +
                $"save to backup: {GetPermanentBackupFilePath(currentIndex)}. Full Error:{e}");
            MoveSaveToPermanentBackup(currentIndex);
        }
    }

    private static void SaveToFile(SerializableSaveProfile profile, string path, string pathTemp=null)
    {
        // Debug.Log($"[File IO] Saving data to file {index}.");

        // BinaryFormatter serialization is obsolete and should not be used. 
        // See https://aka.ms/binaryformatter for more information.
        // It might be worth considering a migration to probably JSON or XML bc 
        // I don't think we mind if players peek at their save profiles.
        BinaryFormatter formatter = new();
        formatter.Binder = AssemblyRemapBinder;

        string initialWritePath = string.IsNullOrEmpty(pathTemp) ? path : pathTemp;
        FileStream stream = new FileStream(initialWritePath, FileMode.Create);

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

        // Copy temp to main file...
        if (!string.IsNullOrEmpty(pathTemp))
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(pathTemp, path);
        }
    }

    /// <summary>
    /// Saves a backup of each profile, to be called when you quit the application. These will be continuously overwritten.
    /// </summary>
    public static void SaveBackups()
    {
        Debug.Log($"[File IO] Creating backups of save profiles...");
        for (int i = 0; i < 3; i++)
        {
            if (saveProfiles[i] != null)
            {
                try 
                {
                    string path = GetBackupFilePath(i);
                    SerializableSaveProfile existingBackup = LoadFromFile(path);
                    if (existingBackup != null && existingBackup.lastSaved == saveProfiles[i].GetLastSaved())
                    {
                        continue;
                    }
                    
                    Debug.Log($"[File IO] Saving backup for profile {i}");
                    SaveToFile(SerializableSaveProfile.FromSaveProfile(saveProfiles[i]), path);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[File IO] Error when saving backups: {e.Message}\n{e}");
                    Debug.Log(e.StackTrace);
                }
            }
        }
        Debug.Log($"[File IO] Done!");
    }

    /// <summary>
    /// Copies a save to backup that will not be overwritten, marked with a date-time.
    /// </summary>
    public static void MoveSaveToPermanentBackup(int index)
    {
        if (Current.GetMovedToPermanent())
        {
            return;
        }

        SerializableSaveProfile profile = SerializableSaveProfile.FromSaveProfile(Current);
        DoMoveSaveToPermanentBackup(profile, index);
        Current.SetMovedToPermanent(true);
        Current.SetLastPermanentBackupTimeInSeconds(Current.GetPlayTimeInSeconds());
    }

    
    public static void DoMoveSaveToPermanentBackup(SerializableSaveProfile profile, int index)
    {
        string path = GetFilePath(index);
        string tempPath = GetFilePathTemp(index);
        string permanentBackupPath = GetPermanentBackupFilePath(index);
        
        // Create directory if it doesn't exist
        FileInfo fileInfo = new FileInfo(permanentBackupPath);
        fileInfo.Directory.Create();

        if (File.Exists(permanentBackupPath))
        {
            Debug.LogError($"[File IO] Error: Tried saving permanent backup to {permanentBackupPath} but file already exists. Skipping...");
            return;
        }

        if (!File.Exists(path))
        {
            if (File.Exists(tempPath))
            {
                Debug.LogWarning($"[File IO] Found file at temp path {tempPath}, using that for backup.");
                File.Copy(tempPath, permanentBackupPath);
                return;
            }

            Debug.LogError($"[File IO] Error: Tried saving permanent backup from {path} but file does not exists. Going to try to generate and copy file...");
            SaveToFile(profile, path, tempPath);
        }
        
        File.Copy(path, permanentBackupPath);
        Debug.Log($"[File IO] Saved permanent backup to {permanentBackupPath}.");
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
        string path = GetFilePath(index);
        return LoadFromFile(path);
    }

    public static SerializableSaveProfile GetBackupSerializableSaveProfile(int index)
    {
        string path = GetBackupFilePath(index);
        return LoadFromFile(path);
    }

    private static SerializableSaveProfile LoadFromFile(string path)
    {
        // Debug.Log($"[File IO] Loading data from file {path}.");

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

    // Create linux specific backups in case Steam cloud deleted saves
    private static void CreateLinuxSpecificBackups(int index)
    {
        string path = GetBackupFilePath(index);
        string linuxPath = GetBackupLinuxSpecificBackupFilePath(index);

        if (File.Exists(path) && !File.Exists(linuxPath))
        {
            File.Copy(path, linuxPath);
        }
    }

    public static void DeleteSaveProfile(int index)
    {
        Debug.Log($"[File IO] Deleting Save profile #{index}!");

        SetProfile(index, null);

        string path = GetFilePath(index);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static void RestoreBackupProfile(int index)
    {
        Debug.Log($"[File IO] Restoring Backup profile #{index}!");
        
        string pathBackup = GetBackupFilePath(index);
        if (!File.Exists(pathBackup))
        {
            Debug.LogError($"[File IO] Aborting: File was not found at path {pathBackup}");
            return;
        }

        string path = GetFilePath(index);
        if (File.Exists(path))
        {
            string pathReplaced = GetBackupReplacedFilePath(index);
            if (File.Exists(pathReplaced))
            {
                File.Delete(pathReplaced);
            }
            File.Move(path, GetBackupReplacedFilePath(index));
        }
        
        File.Move(pathBackup, path);
        SetProfile(index, GetSerializableSaveProfile(index)?.ToSaveProfile());
    }

    public static int GetNumberOfPermanentBackups()
    {
        string permanentBackupPath = GetPermanentBackupFilePath(0);
        
        // Create directory if it doesn't exist
        FileInfo fileInfo = new FileInfo(permanentBackupPath);
        fileInfo.Directory.Create();

        return fileInfo.Directory.GetFiles().Length;
    }

    private static string GetRootPath()
    {
        if (SAVE_SYSTEM_TYPE == SaveSystemType.GDK)
        {
            string path = GDKProxy.GetSaveFilePath();
            if (string.IsNullOrEmpty(path))
            {
                throw new System.Exception("GDK Save File Path is empty!");
            }
            return Path.Join(GDKProxy.GetSaveFilePath(), GDK_GAME_SAVE_CONTAINER_NAME);
        }
        return Application.persistentDataPath;
    }

    public static string GetFilePath(int index)
    {
        return GetRootPath() + string.Format("/slider{0}.cat", index);
    }

    public static string GetFilePathTemp(int index)
    {
        return GetRootPath() + string.Format("/slider{0}-TEMP.cat", index);
    }

    public static string GetBackupFilePath(int index)
    {
        return GetRootPath() + string.Format("/backup-slider{0}.cat", index);
    }

    public static string GetPermanentBackupFilePath(int index)
    {
        System.DateTime dt = System.DateTime.Now;
        return GetRootPath() + string.Format("/backup/slider{0}-{1}.cat", index, dt.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    public static string GetBackupReplacedFilePath(int index)
    {
        return GetRootPath() + string.Format("/replaced-slider{0}.cat", index);
    }

    public static string GetBackupLinuxSpecificBackupFilePath(int index)
    {
        return GetRootPath() + string.Format("/backup-LINUX-slider{0}.cat", index);
    }
}
