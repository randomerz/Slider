using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveProfile
{
    // profile-specific metadata
    private string profileName;
    private string gameVersion;
    private bool completionStatus;
    private float playTimeInSeconds;
    private System.DateTime lastSaved;

    private SerializablePlayer serializablePlayer;
    private Area lastArea;

    private Dictionary<Area, SGridData> areaToSGridData = new Dictionary<Area, SGridData>();

    private Dictionary<string, bool> bools = new Dictionary<string, bool>();
    private Dictionary<string, string> strings = new Dictionary<string, string>();
    private Dictionary<string, int> ints = new Dictionary<string, int>();
    private Dictionary<string, float> floats = new Dictionary<string, float>();
    public AchievementStatistic[] AchievementData { get; set; }

    // Cached stuff
    // nothing bc i dont know what to do bc scenes exist

    public SaveProfile(string profileName)
    {
        this.profileName = profileName;
        strings["Cat"] = profileName;
        strings["CatUpper"] = profileName.ToUpper();
        this.gameVersion = Application.version;
        lastArea = Area.Village;

        foreach (Area area in Area.GetValues(typeof(Area)))
        {
            if (area == Area.None) continue;
            
            areaToSGridData[area] = new SGridData(area);
        }
    }

    #region Getters / Setters
    public string GetProfileName()
    {
        return profileName;
    }

    public string GetGameVersion()
    {
        return gameVersion;
    }

    public void SetGameVersion(string value)
    {
        gameVersion = value;
    }

    public bool GetCompletionStatus()
    {
        return completionStatus;
    }

    public void SetCompletionStatus(bool value)
    {
        completionStatus = value;
    }

    public float GetPlayTimeInSeconds()
    {
        return playTimeInSeconds;
    }

    public void SetPlayTimeInSeconds(float value)
    {
        playTimeInSeconds = value;
    }

    public void AddPlayTimeInSeconds(float time)
    {
        playTimeInSeconds += time;
    }

    public System.DateTime GetLastSaved()
    {
        return lastSaved;
    }

    public void SetLastSaved(System.DateTime value)
    {
        lastSaved = value;
    }

    public SerializablePlayer GetSerializablePlayer()
    {
        return serializablePlayer;
    }

    public void SetSerializeablePlayer(SerializablePlayer value)
    {
        serializablePlayer = value;
    }

    public Area GetLastArea()
    {
        return lastArea;
    }

    public void SetLastArea(Area value)
    {
        lastArea = value;
    }

    public Dictionary<Area, SGridData> GetAreaToSGridData()
    {
        return areaToSGridData;
    }

    public void SetAreaToSGridData(Dictionary<Area, SGridData> value)
    {
        areaToSGridData = value;
    }

    public Dictionary<string, bool> GetBoolsDictionary()
    {
        return bools;
    }

    public void SetBoolsDictionary(Dictionary<string, bool> value)
    {
        bools = value;
    }

    public Dictionary<string, string> GetStringsDictionary()
    {
        return strings;
    }

    public void SetStringsDictionary(Dictionary<string, string> value)
    {
        strings = value;
    }

    public Dictionary<string, int> GetIntsDictionary()
    {
        return ints;
    }

    public void SetIntsDictionary(Dictionary<string, int> value)
    {
        ints = value;
    }

    public Dictionary<string, float> GetFloatsDictionary()
    {
        return floats;
    }

    public void SetFloatsDictionary(Dictionary<string, float> value)
    {
        floats = value;
    }
    #endregion

    public void Save()
    {
        lastSaved = System.DateTime.Now;
        SetBool("isDemoBuild", true);
        SaveSavablesData();
        this.gameVersion = Application.version;
        AchievementData = AchievementManager.GetAchievementData();
    }

    public void Load()
    {
        PreLoadChecks();

        // Tells everyone to load from this profile's data
        LoadSavablesData();

        AchievementManager.OverwriteAchievementData(AchievementData);
    }

    private void PreLoadChecks()
    {
        // If haven't logged on for a few days + specific scenes, spawn with anchor equipped
        List<Area> areasToCheck = new List<Area>{Area.Ocean, Area.Desert, Area.Factory, Area.Mountain};
        bool shouldSpawnWithAnchorEquipped = (System.DateTime.Now - lastSaved).TotalDays > 2 && areasToCheck.Contains(lastArea);
        SetBool("playerSpawnWithAnchorEquipped", shouldSpawnWithAnchorEquipped);
    }

    #region SGrid
    public void SaveSGridData(Area area, SGrid sgrid)
    {
        if (area == Area.None)
        {
            Debug.LogError("Tried saving an area for Region.None");
        }

        // if (!areaToSGridData.ContainsKey(area))
        // {
        //     areaToSGridData[area] = new SGridData(sgrid);
        //     // playerPos[area] = Player.GetPosition();
        //     return;
        // }

        areaToSGridData[area].UpdateGrid(sgrid);
        // Debug.Log("length: " + areaToSGridData[area].grid.Count);
        // playerPos[area] = Player.GetPosition();
    }

    public SGridData GetSGridData(Area area)
    {
        if (area == Area.None)
        {
            Debug.LogError("Tried loading an area for Region.None");
        }

        if (!areaToSGridData.ContainsKey(area))
        {
            return null;
        }

        return areaToSGridData[area];
    }
    #endregion

    #region Savables
    public void SaveSavablesData()
    {
        foreach (ISavable s in GetCachedSavables())
        {
            s.Save();
        }
    }

    public void LoadSavablesData()
    {
        List<ISavable> savables = GetCachedSavables();
        Debug.Log($"[Saves] Loading {savables.Count} savables...");
        // Load Player first
        foreach (ISavable s in savables)
        {
            if (s is Player)
            {
                s.Load(this);
            }
        }

        // then load rest
        foreach (ISavable s in savables)
        {
            if (!(s is Player))
            {
                s.Load(this);
            }
        }
    }

    private List<ISavable> GetCachedSavables()
    {
        List<ISavable> savables = new List<ISavable>();
        foreach (ISavable s in GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<ISavable>())
        {
            savables.Add(s);
        }
        return savables;
    }
    #endregion

    #region Dictionaries
    /// <summary>
    /// Returns the value associated with "name" in keys. If no such value exists, this method returns false by defualt, but can also return true if passed as an argument 
    /// </summary>
    /// <param name="name">The name of the string in the dictionary. Generally, try to follow: "areaBooleanName"</param>
    /// <returns></returns>
    public bool GetBool(string name, bool defaultVal = false)
    {
        return bools.GetValueOrDefault(name, defaultVal);
    }

    public void SetBool(string name, bool value)
    {
        bools[name] = value;
    }

    /// <summary>
    /// Returns "name" if strings dictionary doesn't contain "name" in keys.
    /// </summary>
    /// <param name="name">The name of the string in the dictionary. Generally, try to follow: "areaBooleanName"</param>
    /// <returns></returns>
    public string GetString(string name, string defaultValue = null)
    {
        return strings.GetValueOrDefault(name, defaultValue == null ? name : defaultValue);
    }

    public void SetString(string name, string value)
    {
        strings[name] = value;
    }

    public int GetInt(string name, int defaultValue = 0)
    {
        return ints.GetValueOrDefault(name, defaultValue);
    }

    public void SetInt(string name, int value)
    {
        ints[name] = value;
    }

    public float GetFloat(string name, float defaultValue = 0)
    {
        return floats.GetValueOrDefault(name, defaultValue);
    }

    public void SetFloat(string name, float value)
    {
        floats[name] = value;
    }
    #endregion
}