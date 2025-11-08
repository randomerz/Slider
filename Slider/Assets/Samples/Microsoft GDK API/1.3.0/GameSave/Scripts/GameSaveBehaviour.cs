using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GdkSample_GameSave
{
    public class GameSaveBehaviour : MonoBehaviour
    {
        [Header("Interface")]
        public Text SigninDetailsText;
        public Button m_GenerateNewCharacter;
        public Button m_LevelUpCharacter;
        public Button m_GrantRandomArmor;
        public Button m_ResetCharacter;
        public Button m_InitializeSaveGames;
        public Button m_SaveGame;
        public Button m_LoadGame;
        public Button m_DeleteSaveGame;
        public Button m_QuerySpaceQuota;
        public Button m_QuerySaveContainers;
        public Button m_QuerySaveContainerBlobs;
        public Button m_CloseSaveGamesHandles;

        [Header("Character Data")]
        public Sprite[] m_CharacterPortraits;
        public GameObject m_CharacterPanel;
        public Text m_CharacterName;
        public Text m_CharacterLevel;
        public Text m_CharacterXP;
        public Text m_CharacterArmor;
        public Image m_CharacterPortrait;

        [Header("Save Slots")]
        public GameObject m_SaveGameSlotsPanel;
        public Button[] m_SaveGameSlots;
        private const string k_ContainerPrefix = "GameSlot";
        private const string k_PlayerDataBlobName = "PlayerData";

        private GameSaveManager m_MySimpleSaveGame;
        private bool m_LoggedIn = false;
        private bool m_HasCharacter = false;
        private bool m_InitializedSaveGames = false;
        private bool m_SelectingSlot = false;
        private RPGCharacterData m_CharacterData;

        private void Start()
        {
            if (GDKGameRuntime.TryInitialize())
            {
                UserManager.InitializeAndAddUser(OnUserAdded, OnUserLoggedOut);
            }

            // Disable mouse interaction in case it is not supported.
            if (Mouse.current == null || !Mouse.current.enabled)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            SetupListeners();
            UpdateIntefaceButtons();
            m_CharacterData.Reset();
            UpdateInterfaceCharacterData();
        }

        private void OnDestroy()
        {
            UserManager.Uninitialize();
        }

        private void OnUserAdded()
        {
            m_LoggedIn = true;
            UpdateIntefaceButtons();

            Debug.Log("User added succesfully");
            SigninDetailsText.text = $"User signed in as: {UserManager.CurrentUserGamerTag}";
        }

        private void OnUserLoggedOut()
        {
            if (m_MySimpleSaveGame != null)
            {
                m_MySimpleSaveGame.CloseGameSaveHandles();
            }
            m_LoggedIn = false;
            m_InitializedSaveGames = false;
            m_HasCharacter = false;
            m_SelectingSlot = false;

            UpdateIntefaceButtons();
            SigninDetailsText.text = $"No user signed in";
            if (GDKGameRuntime.Initialized)
            {
                UserManager.InitializeAndAddUser(OnUserAdded, OnUserLoggedOut);
            }
        }

        private void UpdateIntefaceButtons()
        {
            m_GenerateNewCharacter.interactable = m_LoggedIn && !m_SelectingSlot;
            m_LevelUpCharacter.interactable = m_LoggedIn && m_HasCharacter && !m_SelectingSlot;
            m_GrantRandomArmor.interactable = m_LoggedIn && m_HasCharacter && !m_SelectingSlot;
            m_ResetCharacter.interactable = m_LoggedIn && m_HasCharacter && !m_SelectingSlot;

            m_InitializeSaveGames.interactable = m_LoggedIn && !m_InitializedSaveGames && !m_SelectingSlot;
            m_SaveGame.interactable = m_LoggedIn && m_InitializedSaveGames && m_HasCharacter && !m_SelectingSlot;
            m_LoadGame.interactable = m_LoggedIn && m_InitializedSaveGames && !m_SelectingSlot;
            m_DeleteSaveGame.interactable = m_LoggedIn && m_InitializedSaveGames && !m_SelectingSlot;

            m_QuerySpaceQuota.interactable = m_LoggedIn && m_InitializedSaveGames && !m_SelectingSlot;
            m_QuerySaveContainers.interactable = m_LoggedIn && m_InitializedSaveGames && !m_SelectingSlot;
            m_QuerySaveContainerBlobs.interactable = m_LoggedIn && m_InitializedSaveGames && !m_SelectingSlot;
            m_CloseSaveGamesHandles.interactable = m_LoggedIn && m_InitializedSaveGames && !m_SelectingSlot;

            // Reset all player data when logged out
            if (!m_LoggedIn)
            {
                m_CharacterName.text = "NAME: <None>";
                m_CharacterLevel.text = "LEVEL: 00";
                m_CharacterXP.text = "XP: 0";
                m_CharacterPortrait.sprite = null;
                m_CharacterArmor.text = ArmorType.None.ToString();
            }

            if (m_SelectingSlot)
            {
                m_SaveGameSlotsPanel.SetActive(true);
                m_CharacterPanel.SetActive(false);
                for (int i = 0; i < m_SaveGameSlots.Length; i++)
                {
                    m_SaveGameSlots[i].interactable = true;
                }
            }
            else
            {
                for (int i = 0; i < m_SaveGameSlots.Length; i++)
                {
                    m_SaveGameSlots[i].interactable = false;
                }

                m_SaveGameSlotsPanel.SetActive(false);
                m_CharacterPanel.SetActive(true);
            }

            if (EventSystem.current.currentSelectedGameObject == null ||
                !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
            {
                if (m_SelectingSlot)
                {
                    EventSystem.current.SetSelectedGameObject(m_SaveGameSlots[0].gameObject);
                }
                else if (m_LoggedIn)
                {
                    m_GenerateNewCharacter.gameObject.SetActive(true);
                    m_InitializeSaveGames.gameObject.SetActive(true);

                    //m_GenerateNewCharacter.Select();

                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(m_GenerateNewCharacter.gameObject);
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }

        private void SetupListeners()
        {
            m_GenerateNewCharacter.onClick.AddListener(OnGenerateNewCharacter);
            m_LevelUpCharacter.onClick.AddListener(OnLevelUpCharacter);
            m_GrantRandomArmor.onClick.AddListener(OnGrantRandomArmor);
            m_ResetCharacter.onClick.AddListener(OnResetCharacter);

            m_InitializeSaveGames.onClick.AddListener(OnInitializeSaveGames);
            m_SaveGame.onClick.AddListener(OnSaveGame);
            m_LoadGame.onClick.AddListener(OnLoadGame);
            m_DeleteSaveGame.onClick.AddListener(OnDeleteSaveGame);

            m_QuerySpaceQuota.onClick.AddListener(OnQuerySpaceQuota);
            m_QuerySaveContainers.onClick.AddListener(OnQuerySaveContainers);
            m_QuerySaveContainerBlobs.onClick.AddListener(OnQuerySaveContainerBlobs);

            m_CloseSaveGamesHandles.onClick.AddListener(OnCloseSaveGamesHandles);
        }

        private void OnGenerateNewCharacter()
        {
            m_HasCharacter = true;
            m_CharacterData = RPGCharacterData.GenerateRandomCharacter();
            UpdateInterfaceCharacterData();
            UpdateIntefaceButtons();
        }

        private void OnLevelUpCharacter()
        {
            m_CharacterData.LevelUp();
            UpdateInterfaceCharacterData();
        }

        private void OnGrantRandomArmor()
        {
            m_CharacterData.GrantRandomArmor();
            UpdateInterfaceCharacterData();
        }

        private void OnResetCharacter()
        {
            m_HasCharacter = false;
            m_CharacterData.Reset();
            UpdateInterfaceCharacterData();
            UpdateIntefaceButtons();
        }

        private void UpdateInterfaceCharacterData()
        {
            m_CharacterName.text = $"NAME: {m_CharacterData.Name}";
            m_CharacterLevel.text = $"LEVEL: {m_CharacterData.Level:D2}";
            m_CharacterXP.text = $"XP: {m_CharacterData.XP}";
            m_CharacterArmor.text = $"ARMOR: {m_CharacterData.Armor}";
            m_CharacterPortrait.sprite = m_CharacterData.PortraitIndex >= 0 ? m_CharacterPortraits[m_CharacterData.PortraitIndex] : null;
        }

        private void OnInitializeSaveGames()
        {
            m_InitializedSaveGames = false;
            m_MySimpleSaveGame = new GameSaveManager();
            m_MySimpleSaveGame.Initialize(UserManager.CurrentUserHandle, GDKGameRuntime.GameConfigScid, false, OnInitializeSaveGamesComplete);
        }

        private void OnInitializeSaveGamesComplete(int hresult)
        {
            if (HR.FAILED(hresult))
            {
                // Enable offline mode
                if(hresult == HR.E_GS_USER_CANCELED)
                {
                    Debug.Log($"XGameSave initialized in offline mode.");
                    OnQuerySaveContainers();
                    m_InitializedSaveGames = true;
                    UpdateIntefaceButtons();
                    return;
                }
                else
                {
                    Debug.LogError($"Error when initializing XGameSave. HResult 0x{hresult:x} ({HR.NameOf(hresult)})");
                    return;
                }
            }

            Debug.Log("XGameSave initialized succesfully");
            OnQuerySaveContainers();
            m_InitializedSaveGames = true;
            UpdateIntefaceButtons();
        }

        private void OnSaveGame()
        {
            m_SelectingSlot = true;
            UpdateIntefaceButtons();
            for (int i = 0; i < m_SaveGameSlots.Length; i++)
            {
                int slotIndex = i;
                m_SaveGameSlots[slotIndex].onClick.RemoveAllListeners();
                m_SaveGameSlots[slotIndex].onClick.AddListener(() => OnSaveGameSlot(slotIndex));
            }
        }

        private void OnSaveGameSlot(int slotIndex)
        {
            for (int i = 0; i < m_SaveGameSlots.Length; i++)
            {
                m_SaveGameSlots[i].interactable = false;
            }

            var containerName = $"{k_ContainerPrefix}_{slotIndex.ToString()}";
            var blobBufferName = k_PlayerDataBlobName;

            Debug.Log($"Saving m_CharacterData on Slot {slotIndex}. Container: {containerName}. blob Name: {blobBufferName}");

            // The way you obtain bytes is up to you. In the end, the blobs need a byte array type (byte[]).
            // Feel free to use any method you need to. You can save multiple blobs in one go.
            // It cannot ever go more than 12 MB in one go, no matter the amount of blobs.
            var characterDataJson = JsonUtility.ToJson(m_CharacterData);
            var characterDataBytes = System.Text.Encoding.ASCII.GetBytes(characterDataJson);
            var displayName = m_CharacterData.Name;

            m_SaveGameSlots[slotIndex].GetComponentInChildren<Text>().text = $"SLOT {slotIndex + 1}: HAS DATA\n<{displayName}>";
            m_MySimpleSaveGame.GetOrCreateContainer(containerName,
            (hresult) =>
            {
                if (HR.FAILED(hresult))
                {
                    return;
                }

                m_MySimpleSaveGame.SaveGame(displayName,
                                            blobBufferName,
                                            characterDataBytes,
                                            (hresult) => OnSaveGameCompleted(hresult, containerName, blobBufferName));
            });
        }

        private void OnSaveGameCompleted(int hresult, string containerName, string blobBufferName)
        {
            m_SelectingSlot = false;
            UpdateIntefaceButtons();

            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when Saving Game. HResult 0x{hresult:x}");
                return;
            }

            Debug.Log($"Saved data succesfully on container {containerName} and data bufer {blobBufferName}");
        }

        private void OnLoadGame()
        {
            m_SelectingSlot = true;
            UpdateIntefaceButtons();

            for (int i = 0; i < m_SaveGameSlots.Length; i++)
            {
                int slotIndex = i;
                m_SaveGameSlots[slotIndex].onClick.RemoveAllListeners();
                m_SaveGameSlots[slotIndex].onClick.AddListener(() => OnLoadGameSlot(slotIndex));
            }
        }

        private void OnLoadGameSlot(int slotIndex)
        {
            for (int i = 0; i < m_SaveGameSlots.Length; i++)
            {
                m_SaveGameSlots[i].interactable = false;
            }

            var text = m_SaveGameSlots[slotIndex].GetComponentInChildren<Text>().text;
            if (text.Contains("NO DATA"))
            {
                Debug.Log($"Slot {slotIndex}. Contains NO DATA.");

                m_SelectingSlot = false;
                UpdateIntefaceButtons();
                return;
            }

            var containerName = $"{k_ContainerPrefix}_{slotIndex.ToString()}";
            var blobBufferName = k_PlayerDataBlobName;

            Debug.Log($"Loading from Slot {slotIndex}. Container: {containerName}. blob Name: {blobBufferName}");

            m_MySimpleSaveGame.GetOrCreateContainer(containerName,
            (hresult) =>
            {
                if (HR.FAILED(hresult))
                {
                    return;
                }

                m_MySimpleSaveGame.LoadGame(blobBufferName, OnLoadGameCompleted);
            });
        }

        private void OnLoadGameCompleted(int hresult, XGameSaveBlob[] blobs)
        {
            m_SelectingSlot = false;
            UpdateIntefaceButtons();

            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when loading GameSave. HResult 0x{hresult:x}");
                return;
            }

            // For the effects of this sampe, we only expect one blob
            if (blobs.Length > 0)
            {
                Debug.Log($"Loading data buffer successful. Name: {blobs[0].Info.Name} - Size: {blobs[0].Info.Size} bytes");

                // Same as save, you will get a byte array (byte[]).
                // What you do is up to you to convert those bytes to valid data in your game.
                var characterJsonString = System.Text.Encoding.ASCII.GetString(blobs[0].Data);
                m_CharacterData = JsonUtility.FromJson<RPGCharacterData>(characterJsonString);
                m_HasCharacter = true;
                UpdateInterfaceCharacterData();
                UpdateIntefaceButtons();
            }
        }

        private void OnDeleteSaveGame()
        {
            m_SelectingSlot = true;
            UpdateIntefaceButtons();

            for (int i = 0; i < m_SaveGameSlots.Length; i++)
            {
                int slotIndex = i;
                m_SaveGameSlots[slotIndex].onClick.RemoveAllListeners();
                m_SaveGameSlots[slotIndex].onClick.AddListener(() => OnDeleteSaveGameSlot(slotIndex));
            }
        }

        private void OnDeleteSaveGameSlot(int slotIndex)
        {
            var containerName = $"{k_ContainerPrefix}_{slotIndex.ToString()}";
            Debug.Log($"Deleting slot {slotIndex}. Container: {containerName}.");
            m_MySimpleSaveGame.DeleteContainer(containerName,
                        (hresult) =>
                        {
                            m_SelectingSlot = false;
                            UpdateIntefaceButtons();

                            if (HR.FAILED(hresult))
                            {
                                Debug.LogError($"Error when deleting container {containerName}. HResult: {hresult:x}");
                                return;
                            }

                            OnQuerySaveContainers();
                            Debug.Log($"Deleted container {containerName}");
                            OnQuerySaveContainerBlobs();
                        });
        }

        private void OnQuerySpaceQuota()
        {
            m_MySimpleSaveGame.QuerySpaceQuota(OnSpaceQuotaRequested);
        }

        private void OnSpaceQuotaRequested(int hresult, long remainingQuota)
        {
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when requesting remaining space quota. HResult 0x{hresult:x}");
                return;
            }

            Debug.Log($"Remaining Space Quota: {remainingQuota} bytes");
        }

        private void OnQuerySaveContainers()
        {
            Debug.Log("Querying slot containers");
            var containers = m_MySimpleSaveGame.QueryContainers(k_ContainerPrefix);
            Debug.Log($"Found {containers.Length} containers");

            for (int i = 0; i < m_SaveGameSlots.Length; i++)
            {
                m_SaveGameSlots[i].GetComponentInChildren<Text>().text = $"SLOT {i + 1}: NO DATA";
            }

            for (int i = 0; i < containers.Length; i++)
            {
                Debug.Log($"Container {containers[i].Name}.\n    Display Name: {containers[i].DisplayName}. Size: {containers[i].TotalSize} bytes. Blob count: {containers[i].BlobCount}. Last modified {containers[i].LastModifiedTime}.");

                for (int j = 0; j < m_SaveGameSlots.Length; j++)
                {
                    if (containers[i].Name.EndsWith(j.ToString()))
                    {
                        m_SaveGameSlots[j].GetComponentInChildren<Text>().text = $"SLOT {j + 1}: HAS DATA\n<{containers[i].DisplayName}>";
                    }
                }
            }
        }

        private void OnQuerySaveContainerBlobs()
        {
            m_SelectingSlot = true;
            UpdateIntefaceButtons();

            for (int i = 0; i < m_SaveGameSlots.Length; i++)
            {
                int slotindex = i;
                m_SaveGameSlots[slotindex].onClick.RemoveAllListeners();
                m_SaveGameSlots[slotindex].onClick.AddListener(() => OnQueryContainerBlobs(slotindex));
            }
        }

        private void OnQueryContainerBlobs(int slotIndex)
        {
            m_SelectingSlot = false;
            UpdateIntefaceButtons();

            var containerName = $"{k_ContainerPrefix}_{slotIndex.ToString()}";
            Debug.Log($"Querying blobs from Slot {slotIndex}. Container: {containerName}.");

            m_MySimpleSaveGame.GetOrCreateContainer(containerName,
                        (hresult) =>
                        {
                            if (HR.FAILED(hresult))
                            {
                                return;
                            }

                            var blobs = m_MySimpleSaveGame.QueryContainerBlobs(k_PlayerDataBlobName);
                            Debug.Log($"Found {blobs.Length} data blobs");
                            for (int i = 0; i < blobs.Length; i++)
                            {
                                Debug.Log($"Blob {blobs[i].Name}. Size: {blobs[i].Size} bytes.");
                            }
                        });
        }

        private void OnCloseSaveGamesHandles()
        {
            m_MySimpleSaveGame.CloseGameSaveHandles();
            m_InitializedSaveGames = false;
            UpdateIntefaceButtons();
        }
    }
}