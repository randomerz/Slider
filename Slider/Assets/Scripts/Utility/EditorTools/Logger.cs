using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Logger
{
    public static void LogBlockingError(string s)
    {
#if UNITY_EDITOR
        throw new UnityException(s);
#else
        Debug.LogError(s);
#endif
    }
}
