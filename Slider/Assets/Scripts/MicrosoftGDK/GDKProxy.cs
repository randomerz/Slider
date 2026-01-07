using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
#if MICROSOFT_GDK_SUPPORT
using Unity.XGamingRuntime;
#endif

// This code is derived from Microsoft GDK samples and adapted for this project.

public class GDKProxy : Singleton<GDKProxy>
{
#if MICROSOFT_GDK_SUPPORT
    private XUserHandle _userHandle;
    private XblContextHandle _xblContextHandle;
    private XGameSaveFilesWrapper _gameSaveHelper;
    private XUserChangeRegistrationToken _registrationToken;
    private bool _xGameSaveInitialized;

    private const string GAME_SAVE_CONTAINER_NAME = "x_slider_container";

    public static EventHandler OnGameSaveLoaded;

    void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        if (GDKGameRuntime.TryInitialize())
        {
            InitializeAndAddUser();
            SDK.XUserRegisterForChangeEvent(UserChangeEventCallback, out _registrationToken);

            _gameSaveHelper = new XGameSaveFilesWrapper();
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

        if (_registrationToken != null)
        {
            SDK.XUserUnregisterForChangeEvent(_registrationToken);
            _registrationToken = null;
        }
    }

    private void InitializeAndAddUser()
    {
        SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, AddUserComplete);
    }

    private void UserChangeEventCallback(IntPtr _, XUserLocalId userLocalId, XUserChangeEvent eventType)
    {
        if (eventType == XUserChangeEvent.SignedOut)
        {
            Debug.LogWarning("[GDK] User logging out");

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
            Debug.LogWarning($"[GDK] FAILED: Could not signin a user, hResult={hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        _userHandle = userHandle;

        CompletePostSignInInitialization();
    }

    private void CompletePostSignInInitialization()
    {
        string gamertag = string.Empty;

        int hResult = SDK.XUserGetGamertag(_userHandle, XUserGamertagComponent.UniqueModern, out gamertag);
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"[GDK] FAILED: Could not get user tag, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        Debug.Log($"[GDK] SUCCESS: XUserGetGamertag() returned: '{gamertag}'");

        hResult = SDK.XBL.XblContextCreateHandle(_userHandle, out _xblContextHandle);
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"[GDK] FAILED: Could not create context handle, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
        }
        
        _gameSaveHelper.InitializeAsync(
            _userHandle,
            GDKGameRuntime.GameConfigScid,
            XGameSaveInitializeCompleted);
    }

    private void XGameSaveInitializeCompleted(int hResult)
    {
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Initialize game save provider hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        Debug.Log($"[GDK] SUCCESS: Folder initialized: {_gameSaveHelper.FolderResult}");

        _xGameSaveInitialized = true;

        OnGameSaveLoaded?.Invoke(this, EventArgs.Empty);
    }

#region Achievements and Saves

    public static void UnlockAchievement(string achievementId, uint achievementProgress)
    {
        ulong xuid;

        int hResult = SDK.XUserGetId(_instance._userHandle, out xuid);
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"[GDK] FAILED: Could not get user ID, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        // This API will work even when offline.  Offline updates will be posted by the system when connection is
        // re-established even if the title isn’t running. If the achievement has already been unlocked or the progress
        // value is less than or equal to what is currently recorded on the server HTTP_E_STATUS_NOT_MODIFIED (0x80190130L)
        // will be returned.
        SDK.XBL.XblAchievementsUpdateAchievementAsync(
            _instance._xblContextHandle,
            xuid,
            achievementId,
            achievementProgress,
            UnlockAchievementComplete
        );
    }

    private static void UnlockAchievementComplete(int hResult)
    {
        string message = "Achievement Unlocked!";

        if (hResult == HR.HTTP_E_STATUS_NOT_MODIFIED)
        {
            message = "Achievement ALREADY Unlocked!";
        }
        else if (HR.FAILED(hResult))
        {
            Debug.LogError($"[GDK] FAILED: Achievement Update, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        // Debug.Log($"[GDK] SUCCESS: {message}");
    }

    public static bool AreSavesReady()
    {
        return !string.IsNullOrEmpty(GetSaveFilePath()) && _instance._xGameSaveInitialized;
    }

    public static string GetSaveFilePath()
    {
        if (_instance._gameSaveHelper != null && _instance._xGameSaveInitialized)
        {
            return _instance._gameSaveHelper.FolderResult;
        }
        return string.Empty;
    }

#endregion
#endif
}