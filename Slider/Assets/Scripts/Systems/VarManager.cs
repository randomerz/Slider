using UnityEngine;

//This is so we can set bools and stuff in the editor using events.
public class VarManager : MonoBehaviour
{
    public static VarManager instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject); will commenting this break anything? c:
        }
    }

    public bool GetBool(string name)
    {
        return SaveSystem.Current.GetBool(name);
    }

    public void SetBoolOn(string name)
    {
        SaveSystem.Current.SetBool(name, true);
    }

    public void SetBoolOff(string name)
    {
        SaveSystem.Current.SetBool(name, false);
    }
}