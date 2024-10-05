using System;
using System.Collections.Generic;
using System.Linq;
using Localization;

public class ChadChirpDataProvider : Singleton<ChadChirpDataProvider>, IDialogueTableProvider
{
    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        ChadChirpData.ChirpDataList.ToDictionary(d => d.id, d => d.text)
    );

    public static ChadChirpData GetChirpData(string id) => ChadChirpData.ChirpDataList.FirstOrDefault(d => d.id == id);
    public static string GetChirpTranslated(string id) => _instance.GetLocalizedSingle(id);
    public static string GetChirpUsedSaveString(ChadChirpData data) => $"MiscChadChirpUsed_{data.id}";
    public static ChadChirpDataProvider Instance => _instance;

    private void Awake()
    {
        InitializeSingleton(this);
    }

    public static void DoSave()
    {
        foreach (ChadChirpData d in ChadChirpData.ChirpDataList)
        {
            SaveSystem.Current.SetBool(GetChirpUsedSaveString(d), d.hasBeenUsed);
        }
    }

    public static void DoLoad(SaveProfile profile)
    {
        for (int i = 0; i < ChadChirpData.ChirpDataList.Count; i++)
        {
            ChadChirpData d = ChadChirpData.ChirpDataList[i];
            d.hasBeenUsed = profile.GetBool(GetChirpUsedSaveString(d));
        }
    }
}