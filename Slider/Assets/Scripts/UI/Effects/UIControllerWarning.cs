using TMPro;
using UnityEngine;

public class UIControllerWarning : Singleton<UIControllerWarning>
{
    public GameObject warningText;

    private void Awake()
    {
        InitializeSingleton(this);
    }

    public static void SetWarningTextEnabled(bool value)
    {
        if (_instance != null)
        {
            _instance.warningText.SetActive(value);
        }
    }

    void Update()
    {
        // Debug.Log("current selected object: " + UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
    }
}