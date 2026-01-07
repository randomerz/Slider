using System;
using System.Collections.Generic;

#if MICROSOFT_GDK_SUPPORT
using Unity.XGamingRuntime;
using UnityEngine;
#endif

public class XGameSaveFilesWrapper
{
#if MICROSOFT_GDK_SUPPORT
    private XUserHandle m_userHandle;
    private XGameSaveProviderHandle m_gameSaveProviderHandle;

    public string FolderResult { get; private set; }

    ~XGameSaveFilesWrapper()
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

        SDK.XGameSaveFilesGetFolderWithUiAsync(m_userHandle, scid,
            (Int32 hresult, string folderResult) =>
            {
                FolderResult = folderResult;
                callback(hresult);
            });
    }
#endif
}