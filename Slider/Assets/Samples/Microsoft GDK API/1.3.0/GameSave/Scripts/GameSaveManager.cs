using System.Collections.Generic;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.Events;

namespace GdkSample_GameSave
{
    public class GameSaveManager
    {
        private bool m_SyncOnDemand;
        private XGameSaveProviderHandle m_GameSaveProviderHandle;
        private XGameSaveContainerHandle m_GameSaveContainerHandle;
        private XGameSaveUpdateHandle m_GameSaveContainerUpdateHandle;
        private XGameSaveProviderHandle m_MachineStorageProvider;

        public void Initialize(XUserHandle userHandle, string scid, bool syncOnDemand, UnityAction<int> onInitializationComplete)
        {
            m_SyncOnDemand = syncOnDemand;
            SDK.XGameSaveInitializeProviderAsync(
                userHandle,
                scid,
                syncOnDemand,
                (hresult, gameSaveProviderHandle) => OnSaveGameInitialized(hresult, gameSaveProviderHandle, onInitializationComplete));
        }

        private void OnSaveGameInitialized(int hresult, XGameSaveProviderHandle gameSaveProviderHandle, UnityAction<int> onInitializatioComplete)
        {
            // If the user canceled or is offline the XGameSaveProviderHandle is still valid to use in other XGameSave APIs
            if (HR.SUCCEEDED(hresult) || hresult == HR.E_GS_USER_CANCELED )
            {
                m_GameSaveProviderHandle = gameSaveProviderHandle;
            }

            onInitializatioComplete?.Invoke(hresult);
        }

        public void SaveGame(string displayName, string blobBufferName, byte[] blobData, UnityAction<int> onSaveGameCompleted)
        {
            SaveGame(displayName, new string[] { blobBufferName }, new List<byte[]> { blobData }, onSaveGameCompleted);
        }

        /// You can save multiple buffers in one go with this method. Can be useufl for more complex data
        public void SaveGame(string displayName, string[] blobBufferNames, List<byte[]> blobDataList, UnityAction<int> onSaveGameCompleted)
        {
            if (m_GameSaveContainerHandle == null)
            {
                Debug.LogWarning($"You have not created or retrieved a container. Not doing aything.");
                return;
            }

            int hresult = StartContainerUpdate(displayName);
            if (HR.FAILED(hresult))
            {
                onSaveGameCompleted?.Invoke(hresult);
                return;
            }

            for (int i = 0; i < blobBufferNames.Length; i++)
            {
                hresult = SubmitDataBlobToWrite(blobBufferNames[i], blobDataList[i]);
                if (HR.FAILED(hresult))
                {
                    onSaveGameCompleted?.Invoke(hresult);
                    return;
                }
            }

            SubmitGameSaveUpdate(onSaveGameCompleted);
        }

        public void GetOrCreateContainer(string containerName, UnityAction<int> onContainerCreated)
        {
            if (m_GameSaveProviderHandle == null)
            {
                Debug.LogWarning("Game Save Provider not initialized. Not doing anything.");
                onContainerCreated?.Invoke(-1);
                return;
            }

            int hresult = SDK.XGameSaveCreateContainer(m_GameSaveProviderHandle, containerName, out m_GameSaveContainerHandle);
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when creating the {containerName} container. HResult: 0x{hresult:x}");
                onContainerCreated?.Invoke(hresult);
                return;
            }

            Debug.Log($"Container {containerName} obtained or created.");
            onContainerCreated?.Invoke(hresult);
        }

        public int StartContainerUpdate(string containerDisplayName)
        {
            int hresult = SDK.XGameSaveCreateUpdate(m_GameSaveContainerHandle, containerDisplayName, out m_GameSaveContainerUpdateHandle);
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when creating the {containerDisplayName} update process. HResult: 0x{hresult:x}");
                return hresult;
            }

            Debug.Log($"Container {containerDisplayName} update process created.");
            return hresult;
        }

        public int SubmitDataBlobToWrite(string blobName, byte[] data)
        {
            if (m_GameSaveContainerUpdateHandle == null)
            {
                Debug.LogWarning("You have not started a Update save process. not doing anything");
                return -1;
            }

            int hresult = SDK.XGameSaveSubmitBlobWrite(m_GameSaveContainerUpdateHandle, blobName, data);
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when submitting the blob {blobName}. HResult: 0x{hresult:x}");
                return hresult;
            }
            Debug.Log($"Blob {blobName} submitted.");
            return hresult;
        }

        public void SubmitGameSaveUpdate(UnityAction<int> onSubmitGameSaveComplete)
        {
            if (m_GameSaveContainerUpdateHandle == null)
            {
                Debug.LogWarning("You have not started a Update save process. not doing anything");
                onSubmitGameSaveComplete?.Invoke(-1);
                return;
            }

            SDK.XGameSaveSubmitUpdateAsync(
                m_GameSaveContainerUpdateHandle,
                (hresult) => OnSubmitUpdateCompleted(hresult, onSubmitGameSaveComplete));
        }

        private void OnSubmitUpdateCompleted(int hresult, UnityAction<int> onSubmitGameSaveComplete)
        {
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when submitting container updated process. HResult: 0x{hresult:x}");
                onSubmitGameSaveComplete?.Invoke(hresult);
                return;
            }

            Debug.Log($"Update process submitted. Closing Update handle and container.");
            SDK.XGameSaveCloseUpdate(m_GameSaveContainerUpdateHandle);
            SDK.XGameSaveCloseContainer(m_GameSaveContainerHandle);
            onSubmitGameSaveComplete?.Invoke(hresult);
        }

        public void LoadGame(string blobBufferName, UnityAction<int, XGameSaveBlob[]> onLoadGameCompleted)
        {
            if (m_GameSaveContainerHandle == null)
            {
                Debug.LogWarning($"You have not created or retrieved a container. Not doing aything.");
                return;
            }

            LoadGame(new string[] { blobBufferName }, onLoadGameCompleted);
        }

        public void LoadGame(string[] blobBufferNames, UnityAction<int, XGameSaveBlob[]> onLoadGameCompleted)
        {
            if (m_GameSaveContainerHandle == null)
            {
                Debug.LogWarning($"You have not created or retrieved a container. Not doing aything.");
                return;
            }

            SDK.XGameSaveReadBlobDataAsync(m_GameSaveContainerHandle,
                                                        blobBufferNames,
                                                        (hresult, blobs) => OnLoadSaveGameCompleted(hresult, blobs, onLoadGameCompleted));
        }

        private void OnLoadSaveGameCompleted(int hresult, XGameSaveBlob[] blobs, UnityAction<int, XGameSaveBlob[]> onLoadGameCompleted)
        {
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when loading save game. HResult: 0x{hresult:x}");
                onLoadGameCompleted?.Invoke(hresult, null);
                return;
            }
            onLoadGameCompleted?.Invoke(hresult, blobs);
        }

        public void QuerySpaceQuota(UnityAction<int, long> onSpaceQuotaRequested)
        {
            SDK.XGameSaveGetRemainingQuotaAsync(m_GameSaveProviderHandle,
                                                                (hresult, remainingQuota) => onSpaceQuotaRequested?.Invoke(hresult, remainingQuota));
        }

        public XGameSaveContainerInfo[] QueryContainers()
        {
            if (m_GameSaveProviderHandle == null)
            {
                Debug.LogWarning("Game Save Provider not initialized. Not doing anything.");
                return null;
            }

            int hresult = SDK.XGameSaveEnumerateContainerInfo(m_GameSaveProviderHandle, out var containerInfos);
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when enumerating containers. HResult: 0x{hresult:x}");
                return null;
            }

            return containerInfos;
        }

        public XGameSaveContainerInfo[] QueryContainers(string containerPrefix)
        {
            if (m_GameSaveProviderHandle == null)
            {
                Debug.LogWarning("Game Save Provider not initialized. Not doing anything.");
                return null;
            }

            int hresult = SDK.XGameSaveEnumerateContainerInfoByName(m_GameSaveProviderHandle, containerPrefix, out var containerInfos);
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when enumerating containers by name. HResult: 0x{hresult:x}");
                return null;
            }

            return containerInfos;
        }

        public XGameSaveBlobInfo[] QueryContainerBlobs()
        {
            if (m_GameSaveContainerHandle == null)
            {
                Debug.LogWarning($"You have not created or retrieved a container. Not doing aything.");
                return null;
            }

            int hresult = SDK.XGameSaveEnumerateBlobInfo(m_GameSaveContainerHandle, out var blobInfos);
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when enumerating blobs. HResult: 0x{hresult:x}");
                return null;
            }

            return blobInfos;
        }

        public XGameSaveBlobInfo[] QueryContainerBlobs(string blobPrefix)
        {
            if (m_GameSaveContainerHandle == null)
            {
                Debug.LogWarning($"You have not created or retrieved a container. Not doing aything.");
                return null;
            }

            int hresult = SDK.XGameSaveEnumerateBlobInfoByName(m_GameSaveContainerHandle, blobPrefix, out var blobInfos);
            if (HR.FAILED(hresult))
            {
                Debug.LogError($"Error when enumerating blobs by name. HResult: 0x{hresult:x}");
                return null;
            }

            return blobInfos;
        }

        public void DeleteContainer(string containerName, UnityAction<int> onDeleteContainercomplete)
        {
            SDK.XGameSaveDeleteContainerAsync(m_GameSaveProviderHandle, containerName,
                                                            (hresult) => onDeleteContainercomplete?.Invoke(hresult));
        }

        public void CloseGameSaveHandles()
        {
            if (m_GameSaveContainerHandle != null)
            {
                Debug.Log("Closing Container handle");
                SDK.XGameSaveCloseContainer(m_GameSaveContainerHandle);
                m_GameSaveContainerHandle = null;
            }

            if (m_GameSaveContainerUpdateHandle != null)
            {
                Debug.Log("Closing Update handle");
                SDK.XGameSaveCloseUpdate(m_GameSaveContainerUpdateHandle);
                m_GameSaveContainerUpdateHandle = null;
            }

            if (m_GameSaveProviderHandle != null)
            {
                Debug.Log("Closing Game Save provider handle");
                SDK.XGameSaveCloseProvider(m_GameSaveProviderHandle);
                m_GameSaveProviderHandle = null;
            }
        }
    }
}