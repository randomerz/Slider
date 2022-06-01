using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SerializableSaveProfile
{
    public string profileName;
    public float playTimeInSeconds;
    public bool completionStatus;

    public SerializablePlayer serializablePlayer;
    public Area lastArea;

    //private Dictionary<Area, SGridData> areaToSGridData = new Dictionary<Area, SGridData>();
    public Area[] areaToSGridData_Keys;
    public SGridData[] areaToSGridData_Values;

    //private Dictionary<string, bool> bools = new Dictionary<string, bool>();
    //private Dictionary<string, string> strings = new Dictionary<string, string>();
    public string[] bools_Keys;
    public bool[]   bools_Values;
    public string[] strings_Keys;
    public string[] strings_Values;

    public static SerializableSaveProfile FromSaveProfile(SaveProfile saveProfile)
    {
        if (saveProfile == null) return null;

        SerializableSaveProfile ssp = new SerializableSaveProfile();

        ssp.profileName = saveProfile.GetProfileName();
        ssp.playTimeInSeconds = saveProfile.GetPlayTimeInSeconds();
        ssp.completionStatus = saveProfile.GetCompletionStatus();

        ssp.serializablePlayer = saveProfile.GetSerializablePlayer();
        ssp.lastArea = saveProfile.GetLastArea();

        ssp.areaToSGridData_Keys = saveProfile.GetAreaToSGridData().Keys.ToArray();
        ssp.bools_Keys = saveProfile.GetBoolsDictionary().Keys.ToArray();
        ssp.strings_Keys = saveProfile.GetStringsDictionary().Keys.ToArray();

        ssp.areaToSGridData_Values = saveProfile.GetAreaToSGridData().Values.ToArray();
        ssp.bools_Values = saveProfile.GetBoolsDictionary().Values.ToArray();
        ssp.strings_Values = saveProfile.GetStringsDictionary().Values.ToArray();

        return ssp;
    }

    public SaveProfile ToSaveProfile()
    {
        SaveProfile sp = new SaveProfile(profileName);
        
        sp.SetPlayTimeInSeconds(playTimeInSeconds);
        sp.SetCompletionStatus(completionStatus);

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

        return sp;
    }
}
