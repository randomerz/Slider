using System;
using UnityEngine;
using UnityEngine.UI;
#if MICROSOFT_GDK_SUPPORT
using Unity.XGamingRuntime;
#endif

// This code is derived from Microsoft GDK samples and adapted for this project.

public class AchievementsSceneManager : MonoBehaviour
{
    public Text output;

    public Text textGamertag;
    public Text textSandboxId;
    public Text textTitleId;
    public Text textScid;
    public Button buttonUnlockAchievement;

#if MICROSOFT_GDK_SUPPORT
    private XUserHandle _userHandle;
    private XblContextHandle _xblContextHandle;
    private XUserChangeRegistrationToken _registrationToken;

    private const int _100PercentAchievementProgress = 100;

    public bool showAllAchievementsOnStart = false;

    // Start is called before the first frame update
    private void Start()
    {
        buttonUnlockAchievement.interactable = false;
        if (GDKGameRuntime.TryInitialize())
        {
            textSandboxId.text = GDKGameRuntime.GameConfigSandbox;
            textTitleId.text = string.Format($"0x{GDKGameRuntime.GameConfigTitleId}");
            textScid.text = GDKGameRuntime.GameConfigScid;

            InitializeAndAddUser();
            SDK.XUserRegisterForChangeEvent(UserChangeEventCallback, out _registrationToken);
        }
    }

    private void OnDestroy()
    {
        if (_xblContextHandle != null)
        {
            SDK.XBL.XblContextCloseHandle(_xblContextHandle);
            _xblContextHandle = null;
        }

        if (_userHandle != null)
        {
            SDK.XUserCloseHandle(_userHandle);
            _userHandle = null;
        }

        SDK.XUserUnregisterForChangeEvent(_registrationToken);
    }

    private void InitializeAndAddUser()
    {
        SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, AddUserComplete);
    }

    private void UserChangeEventCallback(IntPtr _, XUserLocalId userLocalId, XUserChangeEvent eventType)
    {
        if (eventType == XUserChangeEvent.SignedOut)
        {
            Debug.LogWarning("User logging out");
            textGamertag.text = "User logged out";

            if (_xblContextHandle != null)
            {
                SDK.XBL.XblContextCloseHandle(_xblContextHandle);
                _xblContextHandle = null;
            }

            if (_userHandle != null)
            {
                SDK.XUserCloseHandle(_userHandle);
                _userHandle = null;
            }

            if (GDKGameRuntime.Initialized)
            {
                InitializeAndAddUser();
            }
        }
    }

    private void AddUserComplete(int hResult, XUserHandle userHandle)
    {
        if (HR.FAILED(hResult))
        {
            Debug.LogWarning($"FAILED: Could not signin a user, hResult={hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        _userHandle = userHandle;
        buttonUnlockAchievement.interactable = true;

        CompletePostSignInInitialization();
    }

    private void CompletePostSignInInitialization()
    {
        string gamertag = string.Empty;

        if (textGamertag == null)
        {
            Debug.LogError($"textGamertag is null, set Game Object.");
            return;
        }

        int hResult = SDK.XUserGetGamertag(_userHandle, XUserGamertagComponent.UniqueModern, out gamertag);
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Could not get user tag, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        Debug.Log($"SUCCESS: XUserGetGamertag() returned: '{gamertag}'");
        textGamertag.text = gamertag;

        hResult = SDK.XBL.XblContextCreateHandle(_userHandle, out _xblContextHandle);
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Could not create context handle, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
        }

        AfterStartAndInitialized();
    }

    private void AfterStartAndInitialized()
    {
        if (showAllAchievementsOnStart)
        {
            GetAllAchievementsStatus();
        }
    }

    public void UnlockAchievement()
    {
        UnlockAchievement("1");

        if (output != null)
            output.text = "Unlocking achievement...";
    }

    private void UnlockAchievement(string achievementId)
    {
        ulong xuid;

        int hResult = SDK.XUserGetId(_userHandle, out xuid);
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Could not get user ID, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        // This API will work even when offline.  Offline updates will be posted by the system when connection is
        // re-established even if the title isn’t running. If the achievement has already been unlocked or the progress
        // value is less than or equal to what is currently recorded on the server HTTP_E_STATUS_NOT_MODIFIED (0x80190130L)
        // will be returned.
        SDK.XBL.XblAchievementsUpdateAchievementAsync(
            _xblContextHandle,
            xuid,
            achievementId,
            _100PercentAchievementProgress,
            UnlockAchievementComplete
        );
    }

    private void UnlockAchievementComplete(int hResult)
    {
        string message = "Achievement Unlocked!";

        if (hResult == HR.HTTP_E_STATUS_NOT_MODIFIED)
        {
            message = "Achievement ALREADY Unlocked!";
        }
        else if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Achievement Update, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        Debug.Log($"[GDK] SUCCESS: {message}");

        if (output != null)
            output.text = message;
    }

    public void GetAllAchievementsStatus()
    {
        ulong xuid;

        int hResult = SDK.XUserGetId(_userHandle, out xuid);
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Could not get user ID, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        SDK.XBL.XblAchievementsGetAchievementsForTitleIdAsync(
            _xblContextHandle,
            xuid,
            uint.Parse(GDKGameRuntime.GameConfigTitleId, System.Globalization.NumberStyles.HexNumber),
            XblAchievementType.All,
            false,
            XblAchievementOrderBy.DefaultOrder,
            0,
            32,
            GetAchievementComplete
        );
    }

    private void GetAchievementComplete(int hResult, XblAchievementsResultHandle result)
    {
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Could not get achievement, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        hResult = SDK.XBL.XblAchievementsResultGetAchievements(result, out XblAchievement[] achievements);

        if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Could not get achievement, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }
        
        foreach (XblAchievement achievement in achievements)
        {
            Debug.Log($"Achievement {achievement.Id} {achievement.Name} Unlocked = {achievement.ProgressState == XblAchievementProgressState.Achieved}");
        }

        SDK.XBL.XblAchievementsResultCloseHandle(result);
    }
#endif
}