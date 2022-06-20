using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveProfile
{
    // profile-specific metadata
    private string profileName;
    private float playTimeInSeconds;
    private bool completionStatus;

    private SerializablePlayer serializablePlayer;
    private Area lastArea;

    private Dictionary<Area, SGridData> areaToSGridData = new Dictionary<Area, SGridData>();

    private Dictionary<string, bool> bools = new Dictionary<string, bool>();
    private Dictionary<string, string> strings = new Dictionary<string, string>();

    // Cached stuff
    // nothing bc i dont know what to do bc scenes exist

    public SaveProfile(string profileName)
    {
        this.profileName = profileName;
        strings["Cat"] = profileName;
    }

    #region Getters / Setters

    public string GetProfileName()
    {
        return profileName;
    }

    public float GetPlayTimeInSeconds()
    {
        return playTimeInSeconds;
    }

    public void SetPlayTimeInSeconds(float value)
    {
        playTimeInSeconds = value;
    }

    public bool GetCompletionStatus()
    {
        return completionStatus;
    }

    public void SetCompletionStatus(bool value)
    {
        completionStatus = value;
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

    #endregion

    public void Save()
    {
        // Responsible for going around and saving all the data

        SaveSavablesData();
    }

    public void Load()
    {
        // Tells everyone to load from this profile's data

        LoadSavablesData();
    }

    public void SaveSGridData(Area area, SGrid sgrid)
    {
        if (area == Area.None)
        {
            Debug.LogError("Tried saving an area for Region.None");
        }

        if (!areaToSGridData.ContainsKey(area))
        {
            areaToSGridData[area] = new SGridData(sgrid);
            // playerPos[area] = Player.GetPosition();
            return;
        }

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
        // TODO: actually cache them
        List<ISavable> savables = new List<ISavable>();
        foreach (ISavable s in GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<ISavable>())
        {
            savables.Add(s);
        }
        return savables;
    }

    // public Vector3 GetPlayerPos(Area area) {
    //     return playerPos[area];
    // }

    /// <summary>
    /// Returns false if bools dictionary doesn't contain "name" in keys.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool GetBool(string name)
    {
        if (!bools.ContainsKey(name))
        {
            //Debug.LogWarning("Couldn't find saved variable of name: " + name);
            return false;
        }
        // add a null check here?
        return bools[name];
    }

    public void SetBool(string name, bool value)
    {
        bools[name] = value;
    }
    
    /// <summary>
    /// Returns "name" if strings dictionary doesn't contain "name" in keys.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetString(string name)
    {
        if (!strings.ContainsKey(name))
        {
            //Debug.LogWarning("Couldn't find saved variable of name: " + name);
            return name;
        }
        // add a null check here?
        return strings[name];
    }

    public void SetString(string name, string value)
    {
        strings[name] = value;
    }
}