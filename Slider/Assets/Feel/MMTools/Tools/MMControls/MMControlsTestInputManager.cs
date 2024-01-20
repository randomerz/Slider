using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// This persistent singleton handles the inputs and sends commands to the player in the MMControls demo, its sole purpose is to output debug logs of the various interactions with the demo's inputs
	/// </summary>
	public class MMControlsTestInputManager : MonoBehaviour, MMEventListener<MMSwipeEvent>
	{
		// on start, we force a high target frame rate for a more fluid experience on mobile devices
		protected virtual void Start()
		{
			Application.targetFrameRate = 300;
		}

		public virtual void LeftJoystickMovement(Vector2 movement) { MMDebug.DebugOnScreen("left joystick",movement); }
		public virtual void RightJoystickMovement(Vector2 movement) { MMDebug.DebugOnScreen("right joystick", movement); }
		public virtual void RepositionableJoystickMovement(Vector2 movement) { MMDebug.DebugOnScreen("Repositionable joystick", movement); }
		public virtual void FollowerJoystickMovement(Vector2 movement) { MMDebug.DebugOnScreen("Follower joystick", movement); }

		public virtual void APressed() { MMDebug.DebugOnScreen("Button A Pressed"); }
		public virtual void BPressed() { MMDebug.DebugOnScreen("Button B Pressed"); }
		public virtual void XPressed() { MMDebug.DebugOnScreen("Button X Pressed"); }
		public virtual void YPressed() { MMDebug.DebugOnScreen("Button Y Pressed"); }
		public virtual void RTPressed()	{ MMDebug.DebugOnScreen("Button RT Pressed"); }

		public virtual void APressedFirstTime() { Debug.LogFormat("Button A Pressed for the first time"); }
		public virtual void BPressedFirstTime() { Debug.LogFormat("Button B Pressed for the first time"); }
		public virtual void XPressedFirstTime() { Debug.LogFormat("Button X Pressed for the first time"); }
		public virtual void YPressedFirstTime() { Debug.LogFormat("Button Y Pressed for the first time"); }
		public virtual void RTPressedFirstTime() { Debug.LogFormat("Button RT Pressed for the first time"); }

		public virtual void AReleased()	{ Debug.LogFormat("Button A Released"); }
		public virtual void BReleased()	{ Debug.LogFormat("Button B Released"); }
		public virtual void XReleased()	{ Debug.LogFormat("Button X Released"); }
		public virtual void YReleased()	{ Debug.LogFormat("Button Y Released"); }
		public virtual void RTReleased()	{ Debug.LogFormat("Button RT Released"); }

		public virtual void HorizontalAxisPressed(float value) { MMDebug.DebugOnScreen("horizontal movement",value); }
		public virtual void VerticalAxisPressed(float value) { MMDebug.DebugOnScreen("vertical movement",value); }

		public virtual void LeftPressedFirstTime() { Debug.LogFormat("Button Left Pressed for the first time"); }
		public virtual void UpPressedFirstTime() { Debug.LogFormat("Button Up Pressed for the first time"); }
		public virtual void DownPressedFirstTime() { Debug.LogFormat("Button Down Pressed for the first time"); }
		public virtual void RightPressedFirstTime() { Debug.LogFormat("Button Right Pressed for the first time"); }

		public virtual void LeftReleased()	{ Debug.LogFormat("Button Left Released"); }
		public virtual void UpReleased()	{ Debug.LogFormat("Button Up Released"); }
		public virtual void DownReleased()	{ Debug.LogFormat("Button Down Released"); }
		public virtual void RightReleased()	{ Debug.LogFormat("Button Right Released"); }
		
		public virtual void StickDragged() { Debug.LogFormat("On drag"); }
		public virtual void StickPointerUp() { Debug.LogFormat("On pointer up"); }
		public virtual void StickPointerDown() { Debug.LogFormat("On pointer down"); }

		public virtual void OnMMEvent(MMSwipeEvent swipeEvent)
		{
			Debug.LogFormat("Swipe at a "+ swipeEvent.SwipeAngle+"° angle, and a length of "+ swipeEvent.SwipeLength+" length. Cardinal direction : "+ swipeEvent.SwipeDirection);
			Debug.LogFormat("Swipe origin : " + swipeEvent.SwipeOrigin+ ", swipe end : " + swipeEvent.SwipeDestination);
			Debug.LogFormat("Swipe duration : "+swipeEvent.SwipeDuration);
		}

		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMSwipeEvent>();
		}

		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMSwipeEvent>();
		}
	}
}