using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controls : Singleton<Controls>
{
    private InputSettings _bindings;
    public static InputSettings Bindings 
    { 
        get
        {
            if (_instance._bindings == null)
            {
                _instance.LoadBindings();
            }
            return _instance._bindings;
        }
    }

    private void LoadBindings()
    {
        if (_instance != null)
        {
            var rebinds = PlayerPrefs.GetString("rebinds");
            if (!string.IsNullOrEmpty(rebinds))
            {
                _instance._bindings.LoadBindingOverridesFromJson(rebinds);
            }
        }
    }
}
