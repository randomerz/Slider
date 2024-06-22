using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleRecipeBook : MonoBehaviour
{
    private bool engagedWithTV;

    private BindingBehavior directionalBindingBehavior;
    private BindingBehavior quitBindingBehaviorEsc;
    private BindingBehavior quitBindingBehaviorAction;

    private Vector2 lastDirectionalInput;

    [SerializeField] private JungleRecipeBookUI jungleRecipeBookUI;
    [SerializeField] private JungleRecipeBookCameraControl jungleCameraControl;
    [SerializeField] private JungleRecipeBookHints jungleRecipeBookHints;
    [SerializeField] private PlayerConditionals controllerConditionals; // the controller gameobject not control scheme
    [SerializeField] private Transform playerControllerPosition;

    private void Start() 
    {
        jungleRecipeBookUI.SetCurrentShape(0, withSound: false);
    }

    public void ToggleEngagedWithTV()
    {
        SetEngageWithTV(!engagedWithTV);
    }

    public void SetEngageWithTV(bool value)
    {
        engagedWithTV = value;
        jungleCameraControl.SetLookingAtTV(value);

        if (value)
        {
            jungleRecipeBookHints.StartHintRoutine();

            // Add bindings
            directionalBindingBehavior = Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Move, 
                context => HandleDirectionalInput(context.ReadValue<Vector2>())
            );
            quitBindingBehaviorAction = Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Action, 
                context => SetEngageWithTV(false)
            );
            quitBindingBehaviorEsc = Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Cancel, 
                context => SetEngageWithTV(false)
            );
            // Consume Tab/Q binding
            Controls.Bindings.UI.OpenArtifact.Disable();
            Controls.Bindings.Player.CycleEquip.Disable();
            
            PauseManager.AddPauseRestriction(owner: gameObject);

            // Player stuff
            SetPlayerToControllerPosition();
            Player.SetCanMove(false, canAnimateMovement: false);
            controllerConditionals.DisableConditionals();
            
            AudioManager.PickSound("UI Click").WithVolume(0.3f).WithPitch(1.05f).AndPlay();
        }
        else
        {
            jungleRecipeBookHints.StopHintRoutine();

            // Undo previous bindings
            Controls.UnregisterBindingBehavior(directionalBindingBehavior);
            Controls.UnregisterBindingBehavior(quitBindingBehaviorAction);
            Controls.UnregisterBindingBehavior(quitBindingBehaviorEsc);
            Controls.Bindings.UI.OpenArtifact.Enable();
            Controls.Bindings.Player.CycleEquip.Enable();
            
            PauseManager.RemovePauseRestriction(owner: gameObject);

            Player.SetCanMove(true, canAnimateMovement: true);
            controllerConditionals.EnableConditionals();
            
            AudioManager.PickSound("UI Click").WithVolume(0.25f).WithPitch(1f).AndPlay();
        }

        if (JungleRecipeBookSave.AllRecipesCompleted())
        {
            AchievementManager.SetAchievementStat("completedAllJungleRecipes", 1);
        }
    }

    // Do this instead of WASD separately for controller support?
    private void HandleDirectionalInput(Vector2 input)
    {
        if (input.magnitude < 0.5f)
        {
            lastDirectionalInput = Vector2.zero;
            return;
        }

        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

        // Up
        if (45 <= angle && angle < 135)
        {
            if (lastDirectionalInput == Vector2.up)
                return;

            lastDirectionalInput = Vector2.up;
            jungleRecipeBookUI.IncrementCurrentShape();
        }
        // Left
        else if (135 <= angle && angle < 225)
        {
            if (lastDirectionalInput == Vector2.left)
                return;

            lastDirectionalInput = Vector2.left;
            jungleRecipeBookUI.DecrementCurrentShape();
        }
        // Down
        else if (225 <= angle && angle < 315)
        {
            if (lastDirectionalInput == Vector2.down)
                return;

            lastDirectionalInput = Vector2.down;
            jungleRecipeBookUI.DecrementRecipeDisplay();
        }
        // Right
        else 
        {
            if (lastDirectionalInput == Vector2.right)
                return;

            lastDirectionalInput = Vector2.right;
            jungleRecipeBookUI.IncrementCurrentShape();
        }
    }

    private void SetPlayerToControllerPosition()
    {
        Player.SetPosition(playerControllerPosition.position);
        Player.GetSpriteRenderer().flipX = false;
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, playerControllerPosition.position, playerControllerPosition);
    }

    public void IsEngagedWithTV(Condition c) => c.SetSpec(engagedWithTV);
}
