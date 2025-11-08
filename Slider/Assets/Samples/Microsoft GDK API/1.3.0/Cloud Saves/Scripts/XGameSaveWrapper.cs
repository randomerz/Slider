// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;

using Unity.XGamingRuntime;

namespace GdkSample_CloudSaves
{
    public class XGameSaveWrapper
    {
        private XUserHandle m_userHandle;
        private XGameSaveProviderHandle m_gameSaveProviderHandle;

        ~XGameSaveWrapper()
        {
            SDK.XGameSaveCloseProvider(m_gameSaveProviderHandle);
            SDK.XUserCloseHandle(m_userHandle);
        }

        /// <summary>
        /// Callback invoked when the InitializeAsync async task completes.
        /// </summary>
        /// <param name="hresult">The hresult of the operation.</param>
        public delegate void InitializeCallback(Int32 hresult);

        /// <summary>
        /// Intializes the save game wrapper and may initiate a sync of all of the containers for specified user of the game.
        /// </summary>
        /// <param name="userHandle">Handle of the Xbox Live User whose saves are to be managed.</param>
        /// <param name="scid">Service configuration ID (SCID) of the game whose saves are to be managed.</param>
        /// <param name="callback">Callback invoked when the async task completes. InitializeCallback(Int32 hresult)</param>
        public void InitializeAsync(XUserHandle userHandle, string scid, InitializeCallback callback)
        {
            m_userHandle = null;
            m_gameSaveProviderHandle = null;

            Int32 hr = SDK.XUserDuplicateHandle(userHandle, out m_userHandle);
            if (HR.FAILED(hr))
            {
                callback(hr);
                return;
            }

            SDK.XGameSaveInitializeProviderAsync(m_userHandle, scid, false,
                (Int32 hresult, XGameSaveProviderHandle gameSaveProviderHandle) =>
                {
                    m_gameSaveProviderHandle = gameSaveProviderHandle;
                    callback(hresult);
                });
        }

        /// <summary>
        /// Callback invoked when the GetQuotaCallback async task completes.
        /// </summary>
        /// <param name="hresult">The hresult of the operation.</param>
        /// <param name="remainingQuota">The number of bytes still available within the storage space.</param>
        public delegate void GetQuotaCallback(Int32 hresult, Int64 remainingQuota);

        /// <summary>
        /// Returns the amount of data available to store using the Save game wrapper.
        /// </summary>
        /// <param name="callback">Callback invoked when the async task completes. GetQuotaCallback(Int32 hresult, Int64 remainingQuota)</param>
        public void GetQuotaAsync(GetQuotaCallback callback)
        {
            SDK.XGameSaveGetRemainingQuotaAsync(m_gameSaveProviderHandle,
                new XGameSaveGetRemainingQuotaCompleted(callback));
        }

        /// <summary>
        /// Callback invoked when the QueryContainers async task completes.
        /// </summary>
        /// <param name="hresult">The hresult of the operation.</param>
        /// <param name="containerNames">An array of the container names matching the search conditions.</param>
        public delegate void QueryContainersCallback(Int32 hresult, string[] containerNames);

        /// <summary>
        /// Queries for all of the container names in the storage space for the title that match the search conditions.
        /// </summary>
        /// <param name="containerNamePrefix">Name (or prefix of the name) of the containers to search for.
        /// If you pass in an empty string, the method will return all of the container names.</param>
        /// <param name="callback">Callback invoked when the async task completes. QueryContainersCallback(Int32 hresult, string[] containerNames)</param>
        public void QueryContainers(string containerNamePrefix, QueryContainersCallback callback)
        {
            XGameSaveContainerInfo[] containerInfos;
            Int32 hr = SDK.XGameSaveEnumerateContainerInfoByName(m_gameSaveProviderHandle, containerNamePrefix,
                out containerInfos);

            string[] containerNames = new string[0];
            if (HR.SUCCEEDED(hr))
            {
                containerNames = new string[containerInfos.Length];
                for (int i = 0; i < containerInfos.Length; i++)
                {
                    containerNames[i] = containerInfos[i].Name;
                }
            }

            callback(hr, containerNames);
        }

        /// <summary>
        /// Callback invoked when the QueryContainerBlobs async task completes.
        /// </summary>
        /// <param name="hresult">The hresult of the operation.</param>
        /// <param name="blobInfos">An dictionary of the blob names and sizes matching the search conditions.</param>
        public delegate void QueryBlobsCallback(Int32 hresult, Dictionary<string, UInt32> blobInfos);

        /// <summary>
        /// Queries for all of the blob names and sizes in the storage space for the title that match the search conditions.
        /// </summary>
        /// <param name="containerName">Name of the container for which to return data.</param>
        /// <param name="callback">Callback invoked when the async task completes. QueryBlobsCallback(Int32 hresult, Dictionary<string, UInt32> blobInfos)</param>
        public void QueryContainerBlobs(string containerName, QueryBlobsCallback callback)
        {
            XGameSaveContainerHandle containerHandle;
            Int32 hr = SDK.XGameSaveCreateContainer(m_gameSaveProviderHandle, containerName, out containerHandle);
            if (HR.FAILED(hr))
            {
                callback(hr, new Dictionary<string, UInt32>());
            }

            XGameSaveBlobInfo[] blobInfos;
            hr = SDK.XGameSaveEnumerateBlobInfo(containerHandle, out blobInfos);
            Dictionary<string, UInt32> blobInfosDict = new Dictionary<string, UInt32>();
            if (HR.SUCCEEDED(hr))
            {
                for (int i = 0; i < blobInfos.Length; i++)
                {
                    blobInfosDict.Add(blobInfos[i].Name, blobInfos[i].Size);
                }
            }

            SDK.XGameSaveCloseContainer(containerHandle);

            callback(hr, blobInfosDict);
        }

        /// <summary>
        /// Callback invoked when the QueryContainerBlobs async task completes.
        /// </summary>
        /// <param name="hresult">The hresult of the operation.</param>
        /// <param name="blobData">An array of bytes contained in the blob (file). If there is nothing matching the container name and blob name, this will be an empty array.</param>
        public delegate void LoadCallback(Int32 hresult, byte[] blobData);

        /// <summary>
        /// Loads the data from a given blob (file) that is within the specified container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the blob (file) to load data from.</param>
        /// <param name="callback">Callback invoked when the async task completes. LoadCallback(Int32 hresult, byte[] blobData)</param>
        public void Load(string containerName, string blobName, LoadCallback callback)
        {
            XGameSaveContainerHandle containerHandle;
            Int32 hr = SDK.XGameSaveCreateContainer(m_gameSaveProviderHandle, containerName, out containerHandle);
            if (HR.FAILED(hr))
            {
                callback(hr, default(byte[]));
            }

            string[] blobNames = new string[] { blobName };
            SDK.XGameSaveReadBlobDataAsync(containerHandle, blobNames, (Int32 hresult, XGameSaveBlob[] blobs) =>
            {
                byte[] blobData = default(byte[]);

                if (HR.SUCCEEDED(hresult))
                {
                    if (blobs.Length > 0)
                    {
                        blobData = blobs[0].Data;
                    }
                }

                SDK.XGameSaveCloseContainer(containerHandle);

                callback(hresult, blobData);
            });
        }

        /// <summary>
        /// Callback invoked when the QueryContainerBlobs async task completes.
        /// </summary>
        /// <param name="hresult">The hresult of the operation.</param>
        public delegate void SaveCallback(Int32 hresult);

        /// <summary>
        /// Saves data to the blob (file) within the specified container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the blob (file) to save the data to within the specified container.</param>
        /// <param name="blobData">The bytes that are to be written to the blob (file).</param>
        /// <param name="callback">Callback invoked when the async task completes. SaveCallback(Int32 hresult)</param>
        public void Save(string containerName, string blobName, byte[] blobData, SaveCallback callback)
        {
            Dictionary<string, byte[]> blobsToSave = new Dictionary<string, byte[]>();
            blobsToSave.Add(blobName, blobData);
            Update(containerName, blobsToSave, null, new UpdateCallback(callback));
        }

        /// <summary>
        /// Callback invoked when the QueryContainerBlobs async task completes.
        /// </summary>
        /// <param name="hresult">The hresult of the operation.</param>
        public delegate void DeleteCallback(Int32 hresult);

        /// <summary>
        /// Deletes a container along with all of its blobs (files).
        /// </summary>
        /// <param name="containerName">Name of the container to delete.</param>
        /// <param name="callback">Callback invoked when the async task completes. DeleteCallback(Int32 hresult)</param>
        public void Delete(string containerName, DeleteCallback callback)
        {
            SDK.XGameSaveDeleteContainerAsync(m_gameSaveProviderHandle, containerName,
                new XGameSaveDeleteContainerCompleted(callback));
        }

        /// <summary>
        /// Deletes a specific blob (file) from within the specified container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the blob (file) to delete from the specified container.</param>
        /// <param name="callback">Callback invoked when the async task completes. DeleteCallback(Int32 hresult)</param>
        public void Delete(string containerName, string blobName, DeleteCallback callback)
        {
            Delete(containerName, new string[1] { blobName }, callback);
        }

        /// <summary>
        /// Deletes a specific set of blobs (files) from within the specified container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobNames">Array of blob (file) names to delete from the specified container.</param
        /// <param name="callback">Callback invoked when the async task completes. DeleteCallback(Int32 hresult)</param>
        public void Delete(string containerName, string[] blobNames, DeleteCallback callback)
        {
            Update(containerName, null, blobNames, new UpdateCallback(callback));
        }


        // Helpers
        private delegate void UpdateCallback(Int32 hresult);

        private void Update(string containerName, IDictionary<string, byte[]> blobsToSave, IList<string> blobsToDelete,
            UpdateCallback callback)
        {
            XGameSaveContainerHandle containerHandle;
            Int32 hr = SDK.XGameSaveCreateContainer(m_gameSaveProviderHandle, containerName, out containerHandle);
            if (HR.FAILED(hr))
            {
                callback(hr);
            }

            XGameSaveUpdateHandle updateHandle;
            hr = SDK.XGameSaveCreateUpdate(containerHandle, containerName, out updateHandle);
            if (HR.FAILED(hr))
            {
                SDK.XGameSaveCloseContainer(containerHandle);
                callback(hr);
            }

            if (blobsToSave != null)
            {
                foreach (var blobToSave in blobsToSave)
                {
                    hr = SDK.XGameSaveSubmitBlobWrite(updateHandle, blobToSave.Key, blobToSave.Value);
                    if (HR.FAILED(hr))
                    {
                        SDK.XGameSaveCloseUpdate(updateHandle);
                        SDK.XGameSaveCloseContainer(containerHandle);
                        callback(hr);
                    }
                }
            }

            if (blobsToDelete != null)
            {
                foreach (var blobToDelete in blobsToDelete)
                {
                    hr = SDK.XGameSaveSubmitBlobDelete(updateHandle, blobToDelete);
                    if (HR.FAILED(hr))
                    {
                        SDK.XGameSaveCloseUpdate(updateHandle);
                        SDK.XGameSaveCloseContainer(containerHandle);
                        callback(hr);
                    }
                }
            }

            SDK.XGameSaveSubmitUpdateAsync(updateHandle, (Int32 hresult) =>
            {
                SDK.XGameSaveCloseUpdate(updateHandle);
                SDK.XGameSaveCloseContainer(containerHandle);
                callback(hresult);
            });
        }
    }
} // namespace GdkSample_CloudSaves