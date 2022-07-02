using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
/// <see cref="RegisterBindingBehavior(MonoBehaviour, InputAction, System.Action{InputAction.CallbackContext})"/>
/// which automatically handles enabling and disabling and is both cleaner and less dangerous.
/// </summary>
/// <remarks>Author: Travis</remarks>
public class Controls : Singleton<Controls>
{
    private static InputSettings _bindings;

    /// <summary>
    /// <b>Revisit this when we start looking into controller support!</b>
    /// </summary>
    [SerializeField] private string currentControlScheme = "Keyboard Mouse";

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

    private void OnEnable()
    {
        InitializeSingleton(overrideExistingInstanceWith:this);

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
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            _bindings.LoadBindingOverridesFromJson(rebinds);
        }
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
    public static ManagedBindingBehavior RegisterBindingBehavior(MonoBehaviour owner, InputAction binding, System.Action<InputAction.CallbackContext> behavior)
    {
        ManagedBindingBehavior managedBindingBehavior = new ManagedBindingBehavior(owner, binding, behavior);
        RegisterBindingBehavior(managedBindingBehavior);
        return managedBindingBehavior;
    }
    /// <summary>
    /// This managed binding behavior is automatically removed when the control is triggered while the owner is inactive.
    /// Until that happens, the associated behavior will be triggered every time the binding you pass in is triggered. 
    /// <para/>
    /// This provides a safer and cleaner way to attach behavior to controls than manual subscription. I recommend using this
    /// in all cases where it is possible, which should be pretty much all of them.
    /// </summary>
    public static ManagedBindingBehavior RegisterBindingBehavior(ManagedBindingBehavior managedBindingBehavior)
    {
        InputAction action = _bindings.FindAction(managedBindingBehavior.binding.name);
        action.performed += managedBindingBehavior.Invoke;
        return managedBindingBehavior;
    }

    /// <summary>
    /// Use this to remove a managed binding behavior manually. In most cases, automatic removal when the owner is inactive 
    /// (you get this for free without doing anything) should be fine, but this is here for special cases. 
    /// <see cref="RegisterBindingBehavior(MonoBehaviour, InputAction, System.Action{InputAction.CallbackContext})"/> returns
    /// the <see cref="ManagedBindingBehavior"/>, so save that and pass that in here to remove it.
    /// </summary>
    /// <param name="managedBindingBehavior"></param>
    public static void UnregisterBindingBehavior(ManagedBindingBehavior managedBindingBehavior)
    {
        InputAction action = _bindings.FindAction(managedBindingBehavior.binding.name);
        action.performed -= managedBindingBehavior.Invoke;
    }

    /// <summary>
    /// Use this to get a UI-ready display string for the bindings on the passed in action.
    /// <para/>
    /// <b>Use this instead of action.GetBindingDisplayString because this method considers the current control scheme.</b>
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static string GetBindingDisplayString(InputAction action)
    {
        return action.GetBindingDisplayString(group:_instance.currentControlScheme);
    }
}

/// <summary>
/// Represents a behavior which will be bound to an input binding. In simple terms, use this to add some behavior when a control is triggered.
/// You should probably not need to use this, but it's here just in case. 
/// <para/>
/// Add binding behaviors using <see cref="Controls.RegisterBindingBehavior(MonoBehaviour, InputAction, System.Action{InputAction.CallbackContext})"/>.
/// </summary>
public class ManagedBindingBehavior
{
    public MonoBehaviour owner;
    public InputAction binding;
    public System.Action<InputAction.CallbackContext> behavior;

    public ManagedBindingBehavior(MonoBehaviour owner, InputAction binding, System.Action<InputAction.CallbackContext> behavior)
    {
        this.owner = owner;
        this.binding = binding;
        this.behavior = behavior;
    }

    public void Invoke(InputAction.CallbackContext context)
    {
        if (owner == null || !owner.isActiveAndEnabled)
        {
            Controls.UnregisterBindingBehavior(this);
            return;
        }
        behavior?.Invoke(context);
    }
}
