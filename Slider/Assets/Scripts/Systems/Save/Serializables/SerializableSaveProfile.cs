using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SerializableSaveProfile
{
    public string profileName;
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
    public AchievementStatistic[] achievementData;

    public static SerializableSaveProfile FromSaveProfile(SaveProfile saveProfile)
    {
        if (saveProfile == null) return null;

        SerializableSaveProfile ssp = new SerializableSaveProfile();

        ssp.profileName = saveProfile.GetProfileName();
        ssp.completionStatus = saveProfile.GetCompletionStatus();
        ssp.playTimeInSeconds = saveProfile.GetPlayTimeInSeconds();
        ssp.lastSaved = saveProfile.GetLastSaved();

        ssp.serializablePlayer = saveProfile.GetSerializablePlayer();
        ssp.lastArea = saveProfile.GetLastArea();

        ssp.areaToSGridData_Keys = saveProfile.GetAreaToSGridData().Keys.ToArray();
        ssp.bools_Keys = saveProfile.GetBoolsDictionary().Keys.ToArray();
        ssp.strings_Keys = saveProfile.GetStringsDictionary().Keys.ToArray();
        ssp.ints_Keys = saveProfile.GetIntsDictionary().Keys.ToArray();

        ssp.areaToSGridData_Values = saveProfile.GetAreaToSGridData().Values.ToArray();
        ssp.bools_Values = saveProfile.GetBoolsDictionary().Values.ToArray();
        ssp.strings_Values = saveProfile.GetStringsDictionary().Values.ToArray();
        ssp.ints_Values = saveProfile.GetIntsDictionary().Values.ToArray();

        ssp.achievementData = saveProfile.AchievementData;

        return ssp;
    }

    public SaveProfile ToSaveProfile()
    {
        SaveProfile sp = new SaveProfile(profileName);
        
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

        // Travis: Not sure why I need to do this null check
        if (ints_Keys != null)
        {
            Dictionary<string, int> ints = new Dictionary<string, int>(ints_Keys.Length);
            for (int i = 0; i < ints_Keys.Length; i++)
                ints.Add(ints_Keys[i], ints_Values[i]);
            sp.SetIntsDictionary(ints);
        }

        sp.AchievementData = achievementData;

        return sp;
    }
}
