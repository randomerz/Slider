using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SerializableSaveProfile
{
    public string profileName;
    public string gameVersion;
    public bool completionStatus;
    public float playTimeInSeconds;
    public System.DateTime lastSaved;

    public SerializablePlayer serializablePlayer;
    public Area lastArea;
    public bool inGame;

    public Area[] areaToSGridData_Keys;
    public SGridData[] areaToSGridData_Values;

    public string[] bools_Keys;
    public bool[]   bools_Values;
    public string[] strings_Keys;
    public string[] strings_Values;
    public string[] ints_Keys;
    public int[] ints_Values;
    public string[] floats_Keys;
    public float[] floats_Values;
    public AchievementStatistic[] achievementData;

    public static SerializableSaveProfile FromSaveProfile(SaveProfile saveProfile)
    {
        if (saveProfile == null) return null;

        SerializableSaveProfile ssp = new SerializableSaveProfile();

        ssp.profileName = saveProfile.GetProfileName();
        ssp.gameVersion = saveProfile.GetGameVersion();
        ssp.completionStatus = saveProfile.GetCompletionStatus();
        ssp.playTimeInSeconds = saveProfile.GetPlayTimeInSeconds();
        ssp.lastSaved = saveProfile.GetLastSaved();

        ssp.serializablePlayer = saveProfile.GetSerializablePlayer();
        ssp.lastArea = saveProfile.GetLastArea();

        ssp.areaToSGridData_Keys = saveProfile.GetAreaToSGridData().Keys.ToArray();
        ssp.bools_Keys = saveProfile.GetBoolsDictionary().Keys.ToArray();
        ssp.strings_Keys = saveProfile.GetStringsDictionary().Keys.ToArray();
        ssp.ints_Keys = saveProfile.GetIntsDictionary().Keys.ToArray();
        ssp.floats_Keys = saveProfile.GetFloatsDictionary().Keys.ToArray();

        ssp.areaToSGridData_Values = saveProfile.GetAreaToSGridData().Values.ToArray();
        ssp.bools_Values = saveProfile.GetBoolsDictionary().Values.ToArray();
        ssp.strings_Values = saveProfile.GetStringsDictionary().Values.ToArray();
        ssp.ints_Values = saveProfile.GetIntsDictionary().Values.ToArray();
        ssp.floats_Values = saveProfile.GetFloatsDictionary().Values.ToArray();

        ssp.achievementData = saveProfile.AchievementData;

        return ssp;
    }

    public SaveProfile ToSaveProfile()
    {
        SaveProfile sp = new SaveProfile(profileName);
        
        sp.SetGameVersion(gameVersion);
        sp.SetCompletionStatus(completionStatus);
        sp.SetPlayTimeInSeconds(playTimeInSeconds);
        sp.SetLastSaved(lastSaved);

        sp.SetSerializeablePlayer(serializablePlayer);
        sp.SetLastArea(lastArea);

        Dictionary<Area, SGridData> areaToSGridData = new Dictionary<Area, SGridData>(areaToSGridData_Keys.Length);
        for (int i = 0; i < areaToSGridData_Keys.Length; i++)
            areaToSGridData.Add(areaToSGridData_Keys[i], areaToSGridData_Values[i]);
        sp.SetAreaToSGridData(areaToSGridData);

        Dictionary<string, bool> bools = new Dictionary<string, bool>(bools_Keys.Length);
        for (int i = 0; i < bools_Keys.Length; i++)
            bools.Add(bools_Keys[i], bools_Values[i]);
        sp.SetBoolsDictionary(bools);

        Dictionary<string, string> strings = new Dictionary<string, string>(strings_Keys.Length);
        for (int i = 0; i < strings_Keys.Length; i++)
            strings.Add(strings_Keys[i], strings_Values[i]);
        sp.SetStringsDictionary(strings);

        if (ints_Keys != null)
        {
            Dictionary<string, int> ints = new Dictionary<string, int>(ints_Keys.Length);
            for (int i = 0; i < ints_Keys.Length; i++)
                ints.Add(ints_Keys[i], ints_Values[i]);
            sp.SetIntsDictionary(ints);
        }
        else
        {
            Debug.LogWarning("[SerializableSaveProfile] The saved integers dictionary had no keys. This most likely just means that no ints are being saved.");
        }

        if (floats_Keys != null)
        {
            Dictionary<string, float> floats = new Dictionary<string, float>(floats_Keys.Length);
            for (int i = 0; i < floats_Keys.Length; i++)
                floats.Add(floats_Keys[i], floats_Values[i]);
            sp.SetFloatsDictionary(floats);
        }
        else
        {
            Debug.LogWarning("[SerializableSaveProfile] The saved floats dictionary had no keys. This most likely just means that no floats are being saved.");
        }

        sp.AchievementData = achievementData;

        return sp;
    }
}
