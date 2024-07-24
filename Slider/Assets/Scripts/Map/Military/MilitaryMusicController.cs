using UnityEngine;

public class MilitaryMusicController : MonoBehaviour
{
    private static int musicProgressionParameter;

    private const string LEVEL_PARAM = "MilitaryLevel";
    private const string LOSE_PARAM = "MilitaryLoseTrigger";
    private const string WIN_PARAM = "MilitaryWinTrigger";

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        if (SaveSystem.Current.GetBool(MilitaryWaveManager.BEAT_ALL_ALIENS_STRING))
        {
            SetMilitaryLevel(4);
        }
        else
        {
            SetMilitaryLevel(0);
        }
        AudioManager.SetGlobalParameter(LOSE_PARAM, 0);
        AudioManager.SetGlobalParameter(WIN_PARAM, 0);
    }

    /// <summary>
    /// The levels are as follows:
    /// - [0] Base
    /// - [1] Waves 1, 2
    /// - [2] Waves 3, 4, 5
    /// - [3] Waves 6
    /// - [4] Victory Base
    /// </summary>
    /// <param name="level"></param>
    public static void SetMilitaryLevel(int level)
    {
        AudioManager.SetGlobalParameter(LEVEL_PARAM, level);
        musicProgressionParameter = level;
    }

    public static void DoLoseTrigger()
    {
        AudioManager.SetGlobalParameter(LOSE_PARAM, 1);

        CoroutineUtils.ExecuteAfterDelay(
            () => AudioManager.SetGlobalParameter(LOSE_PARAM, 0),
            SGrid.Current,
            0.1f
        );
    }

    public static void DoWinTrigger()
    {
        AudioManager.SetGlobalParameter(WIN_PARAM, 1);

        CoroutineUtils.ExecuteAfterDelay(
            () => AudioManager.SetGlobalParameter(WIN_PARAM, 0),
            SGrid.Current,
            0.1f
        );
    }
}