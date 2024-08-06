using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NewSavePanelManager : MonoBehaviour
{
    public const string CUTSCENE_SCENE_NAME = "IntroCutscene";

    [SerializeField] private TMP_InputField profileNameTextField;

    private static readonly Action disableUiNavigation = DisableUINavigationToAllowTypingIntoNameField;

    private int saveProfileIndex;
    private BindingBehavior submitBindingBehavior;

    private void OnEnable()
    {
        DisableUINavigationToAllowTypingIntoNameField();
        Controls.OnControlSchemeChanged += disableUiNavigation;
    }

    private void OnDisable()
    {
        Controls.Bindings.UI.Navigate.Enable();
        Controls.OnControlSchemeChanged -= disableUiNavigation;
        Controls.UnregisterBindingBehavior(submitBindingBehavior);
    }

    private static void DisableUINavigationToAllowTypingIntoNameField()
    {
        if (Controls.UsingKeyboardMouseOrKeyboardOnly())
        {
            Controls.Bindings.UI.Navigate.Disable();
            Controls.Bindings.UI.Back.Disable();
            Controls.Bindings.UI.Submit.Disable();
        }
        else
        {
            Controls.Bindings.UI.Navigate.Enable();
            Controls.Bindings.UI.Back.Enable();
            Controls.Bindings.UI.Submit.Enable();

            if (!UINavigationManager.ButtonInCurrentMenuIsSelected())
                UINavigationManager.SelectBestButtonInCurrentMenu();
        }
    }

    public void OpenNewSave(int saveProfileIndex)
    {
        this.saveProfileIndex = saveProfileIndex;
        // gameObject.SetActive(true);

        if (Controls.UsingKeyboardMouse())
        {
            Controls.Bindings.UI.Navigate.Disable();
            profileNameTextField.Select();
        }
        profileNameTextField.text = "";
        MainMenuManager.KeyboardEnabled = true;

        submitBindingBehavior = Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.SubmitOnly, (_) =>
        {
            if (!WasPressedUsingController())
            {
                StartNewGame();
            }
        });
    }

    private bool WasPressedUsingController()
    {
        return Gamepad.current != null && Gamepad.current.buttonSouth.IsPressed();
    }

    public void StartNewGame()
    {
        if (MainMenuManager.KeyboardEnabled)
        {
            string profileName = profileNameTextField.text;

            if (profileName.Length == 0)
                return;

            MainMenuManager.KeyboardEnabled = false;

            Debug.Log($"[File IO] Creating new save profile #{saveProfileIndex} with name {profileName}!");
            SaveSystem.SetProfile(saveProfileIndex, new SaveProfile(profileName));
            SaveSystem.SetCurrentProfile(saveProfileIndex);

            LoadCutscene();
        }
    }

    public void LoadCutscene()
    {
        UIEffects.FadeToBlack(() => {
            SceneManager.LoadScene(CUTSCENE_SCENE_NAME);
            UINavigationManager.CurrentMenu = null;
        }, 1, false);
    }
}
