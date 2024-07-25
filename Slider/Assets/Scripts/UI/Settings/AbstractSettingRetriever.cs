using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractSettingRetriever : MonoBehaviour
{
    public abstract void WriteSettingValue(object value);

    public abstract object ReadSettingValue();
}
