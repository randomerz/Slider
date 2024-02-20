using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

/// <summary>
/// This provides a way to get control bindings without having to load the bindings in multiple places, which is inefficient
/// and likely to lead to issues. Use <see cref="LoadBindings"/> to load the latest bindings stored as JSON in PlayerPrefs
/// and <see cref="Bindings"/> to access them. Note that you do not need to use <see cref="LoadBindings"/> unless you have
/// changed the bindings stored in PlayerPrefs in your code.
/// <para/>
/// When assigning functionality to bindings, first assign them to a variable in your class: InputSettings bindings = Controls.Bindings
/// <br/>
/// Then add your behavior by using: bindings.&lt;Path to the Binding&gt;.performed += context => { &lt;Your Code Here&gt;};
/// <para/>
/// <b>When you do the above, please note that you need to do controls.Enable()/controls.Disable() to make sure the controls are active only when you want them to be!</b>
/// <para/>
/// As an alternative to the above method, I recommend using 
/// <see cref="RegisterBindingBehavior(MonoBehaviour, InputAction, Action{InputAction.CallbackContext})"/>
/// which automatically handles enabling and disabling and is both cleaner and less dangerous.
/// </summary>
/// <remarks>Author: Travis</remarks>
public class Controls : Singleton<Controls>
{
    private static InputSettings _bindings;

    public static Action OnControlSchemeChanged;
    public static string CurrentControlScheme { get; set; } = CONTROL_SCHEME_KEYBOARD_MOUSE;


    public const string CONTROL_SCHEME_KEYBOARD_MOUSE = "Keyboard Mouse";

    public const string CONTROL_SCHEME_CONTROLLER = "Controller";

    /// <summary>
    /// The key used to store binding overrides in PlayerPrefs.
    /// </summary>
    public const string PLAYER_PREFS_REBINDS_KEY = "rebinds";

    /// <summary>
    /// Returns an instance of InputSettings containing our current bindings. If the bindings are not yet loaded, this will load them 
    /// before returning. However, this will not update the bindings to match the latest ones stored inside of PlayerPrefs if the bindings 
    /// were already loaded previously. If you wish to do this, call <see cref="LoadBindings"/> first. This should not be necessary unless 
    /// you are writing new bindings to PlayerPrefs in your code.
    /// </summary>
    public static InputSettings Bindings
    {
        get
        {
            if (_bindings == null)
            {
                LoadBindings();
            }
            return _bindings;
        }
    }

    /// <summary>
    /// Invoked every time a binding behavior is unregistered. Passes the BindingBehavior which was just unregistered. 
    /// Currently only used to support BindingHeldBehaviors ending their coroutines when unregistered, but might prove useful for other stuff.
    /// </summary>
    public static Action<BindingBehavior> OnBehaviorUnregistered;

    private void OnEnable()
    {
        InitializeSingleton(overrideExistingInstanceWith:this);

        // DC: See https://forum.unity.com/threads/input-system-1-4-1-released.1306062/
        // Might be able to remove this line after input system 1.4.3
        InputSystem.settings.SetInternalFeatureFlag("DISABLE_SHORTCUT_SUPPORT", true);

        InputSystem.onDeviceChange += (UnityEngine.InputSystem.InputDevice device, InputDeviceChange change) =>
        {
            if (device is Gamepad && change == InputDeviceChange.Disconnected)
            {
                PauseManager.SetPauseState(true);
            }
        };

        if (_bindings == null)
        {
            LoadBindings();
        }
        _bindings.Enable();
    }
    private void OnDisable()
    {
        _bindings.Disable();
    }

    /// <summary>
    /// Loads the latest bindings from PlayerPrefs. You should only need to call this when you write new bindings into
    /// PlayerPrefs in your code.
    /// </summary>
    public static void LoadBindings()
    {
        if (_bindings == null)
        {
            _bindings = new InputSettings();
        }
        var rebinds = PlayerPrefs.GetString(PLAYER_PREFS_REBINDS_KEY);

        _bindings.Disable();
        _bindings.LoadBindingOverridesFromJson(rebinds);
        _bindings.Enable();
    }

    /// <summary>
    /// This managed binding behavior is automatically removed when the control is triggered while the owner is inactive.
    /// Until that happens, the behavior you pass in will be triggered every time the binding you pass in is triggered. 
    /// <para/>
    /// This provides a safer and cleaner way to attach behavior to controls than manual subscription. I recommend using this
    /// in all cases where it is possible, which should be pretty much all of them.
    /// </summary>
    /// <param name="owner">While this MonoBehavior is disabled, the binding will be removed the next time the control is triggered.</param>
    /// <param name="binding">Use Controls.Bindings.&lt;Binding Path&gt; (eg: Controls.Bindings.UI.Pause)</param>
    /// <param name="behavior">eg: context => DoThing();</param>
    /// <returns>The constructed <see cref="ManagedBindingBehavior"/></returns>
    public static BindingBehavior RegisterBindingBehavior(MonoBehaviour owner, InputAction binding, Action<InputAction.CallbackContext> behavior)
    {
        BindingBehavior managedBindingBehavior = new ManagedBindingBehavior(owner, binding, behavior);
        RegisterBindingBehavior(managedBindingBehavior);
        return managedBindingBehavior;
    }

    /// <summary>
    /// This managed binding behavior is automatically removed when the control is triggered while the owner is inactive.
    /// Until that happens, the associated behavior will be triggered every time the binding you pass in is triggered. 
    /// <para/>
    /// This provides a safer and cleaner way to attach behavior to controls than manual subscription. I recommend using this
    /// in all cases where it is possible, which should be pretty much all of them.
    /// <para/>
    /// Note: a null owner means that the binding behavior will never be automatically
    /// removed. <b>If you add a ManagedBindingBehavior with a null owner, you must remove it manually.</b>
    /// </summary>
    public static BindingBehavior RegisterBindingBehavior(BindingBehavior bindingBehavior)
    {
        InputAction action = _bindings.FindAction(bindingBehavior.binding.name);
        action.performed += bindingBehavior.Invoke;
        return bindingBehavior;
    }

    /// <summary>
    /// Use this to remove a binding behavior manually. In most cases, automatic removal when the owner is inactive 
    /// (you get this for free without doing anything) should be fine, but this is here for special cases. 
    /// <see cref="RegisterBindingBehavior(MonoBehaviour, InputAction, Action{InputAction.CallbackContext})"/> returns
    /// the <see cref="ManagedBindingBehavior"/>, so save that and pass that in here to remove it.
    /// </summary>
    /// <param name="bindingBehavior"></param>
    public static void UnregisterBindingBehavior(BindingBehavior bindingBehavior)
    {
        InputAction action = _bindings.FindAction(bindingBehavior.binding.name);
        if (action == null)
        {
            Debug.Log("Couldn't find action. Did the editor exit play mode?");
            return;
        }
        action.performed -= bindingBehavior.Invoke;
        OnBehaviorUnregistered?.Invoke(bindingBehavior);
    }

    /// <summary>
    /// Returns the control with no underscores and with a space before each capital letter.
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public static string ControlDisplayString(Control control)
    {
        string controlDisplayString = control.ToString().Replace("_", "");
        IEnumerable<string> controlDisplayStringWithSpaces = controlDisplayString.Select(c => char.IsUpper(c) ? $" {c}" : c.ToString());
        return string.Concat(controlDisplayStringWithSpaces)
                     .TrimStart(); // Remove the space added before the first character (e.g. "MoveLeft" -> " Move Left" -> "Move Left")
    }

    /// <summary>
    /// Use this to get a UI-ready display string for the current binding for the passed in control.
    /// </summary>
    /// <param name="control">The control to get the current binding for</param>
    /// <param name="forSpecificScheme">If specified, returns the control for the particular group 
    /// (CONTROL_SCHEME_KEYBOARD_MOUSE or CONTROL_SCHEME_CONTROLLER.) Otherwise, returns the binding
    /// for the current ControlScheme.</param>
    /// <returns></returns>
    public static string BindingDisplayString(Control control, string forSpecificScheme = null)
    {
        InputAction inputActionForControl = InputActionForControl(control);
        if (control.ToString().Contains("Move"))
        {
            return InputActionRebindingExtensions.GetBindingDisplayString(inputActionForControl, BindingIndex(control));
        }
        else
        {
            string controlScheme = forSpecificScheme ?? CurrentControlScheme;
            return inputActionForControl?.GetBindingDisplayString(group: controlScheme);
        }
    }

    public static InputAction InputActionForControl(Control control)
    {
        string controlActionSearchString = InputActionSearchString(control);
        return Bindings.FindAction(controlActionSearchString);
    }

    private static string InputActionSearchString(Control control)
    {
        string controlAsString = control.ToString();
        if (controlAsString.Contains("Move"))
        {
            return "Move";
        }
        return controlAsString;
    }

    /// <summary>
    /// For composite actions (in our case, just movement), we need to target a specific binding index to overwrite a particular direction.
    /// Our Controls enum is set up to let us use the int value of the movement controls as the binding index (Up = 1, Down = 2, etc.)
    /// For all other bindings, we just return 0 since there is only one binding to change.
    /// <b>Note: This assumes all of the keyboard bindings are the first in the list for each action 
    /// (e.g for movement "W" needs to come before "Left Joystick".) If that isn't the case, this will break.</b>
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public static int BindingIndex(Control control)
    {
        if (InputActionSearchString(control) == "Move")
        {
            return (int)control;
        } 
        else
        {
            return 0;
        }
    }

    public static Coroutine StartCoroutineOnInstance(IEnumerator coroutine)
    {
        return _instance.StartCoroutine(coroutine);
    }

    public static void StopCoroutineOnInstance(Coroutine coroutine)
    {
        if (coroutine != null && _instance != null)
            _instance.StopCoroutine(coroutine);
    }

    /// <summary>
    /// Resets all control bindings to the default controls and deletes any overrides stored in PlayerPrefs.
    /// </summary>
    public static void ResetAllBindingsToDefaults()
    {
        _bindings.RemoveAllBindingOverrides();
        PlayerPrefs.SetString(PLAYER_PREFS_REBINDS_KEY, _bindings.SaveBindingOverridesAsJson());
        LoadBindings();
    }
}

/// <summary>
/// Represents a behavior which will be bound to an input binding. In simple terms, use this to add some behavior when a control is triggered.
/// This binding will be automatically removed the first time the binding is triggered while the owner MonoBehavior is null or disabled.
/// <para/>
/// Add binding behaviors using <see cref="Controls.RegisterBindingBehavior(MonoBehaviour, InputAction, Action{InputAction.CallbackContext})"/>.
/// </summary>
public class ManagedBindingBehavior : BindingBehavior
{
    public MonoBehaviour owner;

    public ManagedBindingBehavior(MonoBehaviour owner, InputAction binding, Action<InputAction.CallbackContext> behavior, 
                                  bool isEnabled = true)
        : base(binding, behavior, isEnabled)
    {
        this.owner = owner;
    }

    public override void Invoke(InputAction.CallbackContext context)
    {
        if (isEnabled)
        {
            if (owner == null || !owner.isActiveAndEnabled)
            {
                Controls.UnregisterBindingBehavior(this);
                return;
            }
            behavior?.Invoke(context);
        }
    }
}

/// <summary>
/// Represents a behavior which will be bound to an input binding. In simple terms, use this to add some behavior when a control is triggered.
/// <para/>
/// If you want this binding behavior to be automatically removed when an owner MonoBehavior is disabled, use <see cref="ManagedBindingBehavior"/>.
/// <br/>Generally speaking, you should prefer <see cref="ManagedBindingBehavior"/> and only use this when the managed option does not work for your use case.
/// <para/>
/// Add binding behaviors using <see cref="Controls.RegisterBindingBehavior(MonoBehaviour, InputAction, Action{InputAction.CallbackContext})"/>.
/// </summary>
public class BindingBehavior
{
    public InputAction binding;
    public Action<InputAction.CallbackContext> behavior;
    public bool isEnabled;

    public BindingBehavior(InputAction binding, Action<InputAction.CallbackContext> behavior, bool isEnabled = true)
    {
        this.binding = binding;
        this.behavior = behavior;
        this.isEnabled = isEnabled;
    }

    public virtual void Invoke(InputAction.CallbackContext context)
    {
        if (isEnabled)
        {
            behavior?.Invoke(context);
        }
    }
}

/// <summary>
/// Represents a binding behavior which supports triggering events on the initial press, 
/// after the button is held for a period of time, each frame while the button is held, and when the button is released early.
/// <para/>
/// The onButtonReleasedEarly and onEachFrameWhileButtonHeld events pass a float representing the duration of the hold up to that point.
/// </summary>
public class BindingHeldBehavior : BindingBehavior
{
    public float holdDuration;
    public Action<InputAction.CallbackContext> onHoldStarted;
    public Action<InputAction.CallbackContext> onHoldCompleted;
    public Action<float> onButtonReleasedEarly;
    public Action<float> onEachFrameWhileButtonHeld;

    private Coroutine activeBindingHeldCoroutine;

    public BindingHeldBehavior(InputAction binding, 
                               float holdDuration,
                               Action<InputAction.CallbackContext> onHoldStarted = null,
                               Action<InputAction.CallbackContext> onHoldCompleted = null,
                               Action<float> onButtonReleasedEarly = null,
                               Action<float> onEachFrameWhileButtonHeld = null,
                               bool isEnabled = true)
        : base(binding, onHoldCompleted, isEnabled)
    {
        this.holdDuration = holdDuration;
        this.onHoldStarted = onHoldStarted;
        this.onHoldCompleted = onHoldCompleted;
        this.onButtonReleasedEarly = onButtonReleasedEarly;
        this.onEachFrameWhileButtonHeld = onEachFrameWhileButtonHeld;

        // Handle ending the coroutine early if the behavior gets removed to make Boomo happy
        Controls.OnBehaviorUnregistered += (BindingBehavior behaviorThatWasUnregistered) =>
        {
            if (behaviorThatWasUnregistered == this)
            {
                Controls.StopCoroutineOnInstance(activeBindingHeldCoroutine);
            }
        };
    }

    public override void Invoke(InputAction.CallbackContext context)
    {
        activeBindingHeldCoroutine = Controls.StartCoroutineOnInstance(ICheckForBindingHeld(context));
    }

    private void ActualInvoke(InputAction.CallbackContext context)
    {
        behavior?.Invoke(context);
    }

    private IEnumerator ICheckForBindingHeld(InputAction.CallbackContext context)
    {
        if (binding.controls[0] == null)
        {
            Debug.LogError("BindingHeldBehavior was attached to a control with a null binding");
            yield return null;
        }

        onHoldStarted?.Invoke(context);

        float timeSinceButtonPressed = 0;
        while (AnyBindingControlIsHeld() || timeSinceButtonPressed < holdDuration)
        {
            timeSinceButtonPressed += Time.deltaTime;
            onEachFrameWhileButtonHeld?.Invoke(timeSinceButtonPressed);
            if (!AnyBindingControlIsHeld())
            {
                onButtonReleasedEarly?.Invoke(timeSinceButtonPressed);
                break;
            }
            if (timeSinceButtonPressed >= holdDuration)
            {
                ActualInvoke(context);
                break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private bool AnyBindingControlIsHeld()
    {
        return binding.controls.ToList().Where((control) => control.IsPressed()).Count() > 0;
    }
}

public enum Control
{
    Move_Up = 1,
    Move_Down = 2,
    Move_Left = 3,
    Move_Right = 4,
    Action,
    CycleEquip,
    OpenArtifact,
    Pause,
    AltViewHold
}
