using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.UI;

namespace GdkSample_CloudSaves
{
    public class CloudSavesSceneManager : MonoBehaviour
    {
        public Text output;

        public Text textGamertag;
        public Text textSandboxId;
        public Text textTitleId;
        public Text textScid;

        private XUserHandle _userHandle;
        private XblContextHandle _xblContextHandle;
        private XGameSaveWrapper _gameSaveHelper;
        private XUserChangeRegistrationToken _registrationToken;
        private bool _xGameSaveInitialized;

        private const string _GameSaveContainerName = "x_game_save_default_container";
        private const string _GameSaveBlobName = "x_game_save_default_blob";

        private string _helperText;

        [Serializable]
        private class PlayerSaveData
        {
            public string name;
            public int level;
        }

        private PlayerSaveData playerSaveData;

        public class GameSaveLoadedArgs : System.EventArgs
        {
            public byte[] Data { get; private set; }

            public GameSaveLoadedArgs(byte[] data)
            {
                this.Data = data;
            }
        }

        public delegate void OnGameSaveLoadedHandler(object sender, GameSaveLoadedArgs e);

#pragma warning disable 0067 // Called when MICROSOFT_GAME_CORE is defined

        public event OnGameSaveLoadedHandler OnGameSaveLoaded;

#pragma warning restore 0067

        // Start is called before the first frame update
        private void Start()
        {
            _helperText = output.text;

            // Create some data
            playerSaveData = new PlayerSaveData();
            playerSaveData.name = "Jane Doe";
            playerSaveData.level = 2;

            // Do initialization
            if (GDKGameRuntime.TryInitialize())
            {
                textSandboxId.text = GDKGameRuntime.GameConfigSandbox;
                textTitleId.text = string.Format($"0x{GDKGameRuntime.GameConfigTitleId}");
                textScid.text = GDKGameRuntime.GameConfigScid;

                InitializeAndAddUser();
                SDK.XUserRegisterForChangeEvent(UserChangeEventCallback, out _registrationToken);

                _gameSaveHelper = new XGameSaveWrapper();
                OnGameSaveLoaded += OnGameSaveLoadedCallback;
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

        private void AddUserComplete(int hResult, XUserHandle userHandle)
        {
            if (HR.FAILED(hResult))
            {
                Debug.LogWarning($"FAILED: Could not signin a user, hResult={hResult:X} ({HR.NameOf(hResult)})");
                return;
            }

            _userHandle = userHandle;

            CompletePostSignInInitialization();
        }

        private void UserChangeEventCallback(IntPtr _, XUserLocalId userLocalId, XUserChangeEvent eventType)
        {
            if (eventType == XUserChangeEvent.SignedOut)
            {
                Debug.LogWarning("User logging out");
                textGamertag.text = "User logged out";
                output.text = _helperText;

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
                Debug.LogWarning($"FAILED: Could not get user tag, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
                return;
            }

            Debug.Log($"SUCCESS: XUserGetGamertag() returned: '{gamertag}'");
            textGamertag.text = gamertag;

            hResult = SDK.XBL.XblContextCreateHandle(_userHandle, out _xblContextHandle);
            if (HR.FAILED(hResult))
            {
                Debug.LogError($"FAILED: Could not create context handle, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            }

            _gameSaveHelper.InitializeAsync(
                _userHandle,
                GDKGameRuntime.GameConfigScid,
                XGameSaveInitializeCompleted);
        }

        public void SaveClicked()
        {
            if (_userHandle == null || _xGameSaveInitialized == false)
            {
                return;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, playerSaveData);
                SaveData(memoryStream.ToArray());
                output.text = "\n Saved game data:" +
                              "\n Name: " + playerSaveData.name +
                              "\n Level: " + playerSaveData.level;
            }
        }

        private void OnGameSaveLoadedCallback(object sender, GameSaveLoadedArgs saveData)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(saveData.Data))
            {
                object playerSaveDataObj = binaryFormatter.Deserialize(memoryStream);
                playerSaveData = playerSaveDataObj as PlayerSaveData;
                output.text = "\n Loaded save game:" +
                              "\n Name: " + playerSaveData.name +
                              "\n Level: " + playerSaveData.level;
            }
        }

        private void XGameSaveInitializeCompleted(int hResult)
        {
            if (HR.FAILED(hResult))
            {
                Debug.LogError($"FAILED: Initialize game save provider");
                return;
            }

            _xGameSaveInitialized = true;
        }

        public void SaveData(byte[] data)
        {
            _gameSaveHelper.Save(
                _GameSaveContainerName,
                _GameSaveBlobName,
                data,
                GameSaveSaveCompleted);
        }

        private void GameSaveSaveCompleted(int hResult)
        {
            if (HR.FAILED(hResult))
            {
                if (hResult == HR.E_GS_USER_NOT_REGISTERED_IN_SERVICE)
                {
                    Debug.LogError($"FAILED: User may be logging out x{hResult:X} ({HR.NameOf(hResult)})");
                }
                else
                {
                    Debug.LogError($"FAILED: Game save submit, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
                }
                return;
            }

            Debug.Log($"SUCCESS: Game save submit update complete");
        }

        public void LoadClicked()
        {
            if (_userHandle == null || _xGameSaveInitialized == false)
            {
                return;
            }

            _gameSaveHelper.Load(
                _GameSaveContainerName,
                _GameSaveBlobName,
                GameSaveLoadCompleted);
        }

        private void GameSaveLoadCompleted(int hResult, byte[] savedData)
        {
            if (HR.FAILED(hResult))
            {
                if (hResult == HR.E_GS_USER_NOT_REGISTERED_IN_SERVICE)
                {
                    Debug.LogError($"FAILED: User may be logging out x{hResult:X} ({HR.NameOf(hResult)})");
                }
                else
                {
                    Debug.LogError($"FAILED: Game load, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
                }
                return;
            }

            if (OnGameSaveLoaded != null)
            {
                OnGameSaveLoaded(this, new GameSaveLoadedArgs(savedData));
            }

            Debug.Log($"SUCCESS: Game save load complete");
        }
    }
} // namespace GdkSample_CloudSaves