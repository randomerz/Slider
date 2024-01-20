using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Linq;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Debug helpers
	/// </summary>
	public static class MMDebug 
	{
		#region Commands

		// the cached list of debug log commands
		private static MethodInfo[] _commands;
		// the max length of the log
		private static readonly int _logHistoryMaxLength = 256;

		#if UNITY_EDITOR
		private static bool _debugDrawEnabledSet = false;
		#endif
		private static bool _debugDrawEnabled = false;
		private static bool _debugLogEnabled = false;
		private static bool _debugLogEnabledSet = false;

		/// <summary>
		/// Returns a list of all the debug command lines found in the project's assemblies
		/// </summary>
		public static MethodInfo[] Commands
		{
			get
			{
				if (_commands == null)
				{
					_commands = AppDomain.CurrentDomain.GetAssemblies()
						.SelectMany(
							m => m.GetTypes().SelectMany(
								n => n.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
									.Where(o => o.GetCustomAttribute<MMDebugLogCommandAttribute>() != null))).ToArray();
				}

				return _commands;
			}
		}

		/// <summary>
		/// Tries to input a command
		/// </summary>
		/// <param name="command"></param>
		public static void DebugLogCommand(string command)
		{
			// if the command is empty we output an empty line
			if (command == string.Empty || command == null)
			{
				LogCommand("", "#ff2a00");
				return; 
			}

			// we split around spaces
			string[] splitCommand = command.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
			if (splitCommand == null || splitCommand.Length == 0)
			{
				LogCommand("Empty command", "#ff2a00");
				return;
			}
            
			// we check if the first command exists
			string commandFirst = MMString.UppercaseFirst(splitCommand[0]);
			MethodInfo[] methods = Commands.Where(m => m.Name == commandFirst).ToArray();
			if (methods.Length == 0)
			{
				LogCommand("Command " + commandFirst + " not found.", "#ff2a00");
				return;
			}

			MethodInfo commandInfo;
			object[] parameters = null;

			if (splitCommand.Length > 1)
			{ 
				// if there are arguments
				commandInfo = methods.Where(m => m.GetParameters().Length > 0).FirstOrDefault();

				if (commandInfo == null)
				{
					LogCommand("A version of command " + commandFirst + " with arguments could not be found. Maybe try without arguments.", "#ff2a00");
					return;
				}

				MMDebugLogCommandArgumentCountAttribute argumentAttribute = commandInfo.GetCustomAttributes<MMDebugLogCommandArgumentCountAttribute>(true).FirstOrDefault();
				if (argumentAttribute != null && argumentAttribute.ArgumentCount > splitCommand.Length - 1)
				{ 
					LogCommand("A version of command " + commandFirst + " needs at least " + argumentAttribute.ArgumentCount + " arguments.", "#ff2a00");
					return;
				}

				parameters = new object[] { splitCommand };
			}
			else
			{ 
				// if there are no arguments 
				commandInfo = methods.Where(m => m.GetParameters().Length == 0).FirstOrDefault();

				if (commandInfo == null)
				{
					LogCommand("A version of command " + commandFirst + " without arguments could not be found.", "#ff2a00");
					return;
				}
			}

			LogCommand(command, "#FFC400");
			methods[0].Invoke(null, parameters);
		}

		/// <summary>
		/// Logs the command, adding it to the log history and triggers an event
		/// </summary>
		/// <param name="command"></param>
		/// <param name="color"></param>
		private static void LogCommand(string command, string color)
		{
			DebugLogItem item = new DebugLogItem(command, color, Time.frameCount, Time.time, 3, true);
			LogHistory.Add(item);
			MMDebugLogEvent.Trigger(new DebugLogItem(null, "", Time.frameCount, Time.time, 0, false));
		}

		#endregion

		#region DebugLog

		/// <summary>
		/// A struct used to store log items
		/// </summary>
		public struct DebugLogItem
		{
			public object Message;
			public string Color;
			public int Framecount;
			public float Time;
			public int TimePrecision;
			public bool DisplayFrameCount;

			public DebugLogItem(object message, string color, int framecount, float time, int timePrecision, bool displayFrameCount)
			{
				Message = message;
				Color = color;
				Framecount = framecount;
				Time = time;
				TimePrecision = timePrecision;
				DisplayFrameCount = displayFrameCount;
			}
		}

		/// <summary>
		/// A list of all the debug logs (up to DebugLogMaxLength entries)
		/// </summary>
		public static List<DebugLogItem> LogHistory = new List<DebugLogItem>(_logHistoryMaxLength);

		/// <summary>
		/// Returns a string with all log history condensed
		/// </summary>
		public static string LogHistoryText
		{
			get
			{
				string colorPrefix = "";
				string colorSuffix = "";

				StringBuilder log = new StringBuilder();
				for (int i = 0; i < LogHistory.Count; i++)
				{
					// colors
					if (!string.IsNullOrEmpty(LogHistory[i].Color))
					{
						colorPrefix = "<color=" + LogHistory[i].Color + ">";
						colorSuffix = "</color>";
					}

					// build output
					if (LogHistory[i].DisplayFrameCount)
					{
						log.Append("<color=#82d3f9>[" + LogHistory[i].Framecount + "]</color> ");
					}
					log.Append("<color=#f9a682>[" + MMTime.FloatToTimeString(LogHistory[i].Time, false, true, true, true) + "]</color> ");
					log.Append(colorPrefix + LogHistory[i].Message + colorSuffix);
					log.Append(System.Environment.NewLine);
				}
				return log.ToString();
			}
		}

		/// <summary>
		/// Clears the debug log
		/// </summary>
		public static void DebugLogClear()
		{
			LogHistory.Clear();
			MMDebugLogEvent.Trigger(new DebugLogItem(null, "", Time.frameCount, Time.time, 0, false));
		}

		/// <summary>
		/// Outputs the message object to the console, prefixed with the current timestamp
		/// </summary>
		/// <param name="message">Message.</param>
		public static void DebugLogTime(object message, string color = "", int timePrecision = 3, bool displayFrameCount = true)
		{
			if (!DebugLogsEnabled)
			{
				return;
			}

			string callerObjectName = new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name;
			color = (color == "") ? "#00FFFF" : color;
            
			// colors
			string colorPrefix = "";
			string colorSuffix = "";
			if (!string.IsNullOrEmpty(color))
			{
				colorPrefix = "<color=" + color + ">";
				colorSuffix = "</color>";
			}

			// build output
			string output = "";
			if (displayFrameCount)
			{
				output += "<color=#82d3f9>[f" + Time.frameCount + "]</color> ";
			}
			output += "<color=#f9a682>[" + MMTime.FloatToTimeString(Time.time, false, true, true, true) + "]</color> ";
			output += callerObjectName + " : ";
			output += colorPrefix + message + colorSuffix;

			// we output to the console
			Debug.Log(output);

			// we log to the MM console
			DebugLogItem item = LogDebugToConsole(message, color, timePrecision, displayFrameCount);

		}

		/// <summary>
		/// Logs the specified message to the console
		/// </summary>
		/// <param name="message"></param>
		/// <param name="color"></param>
		/// <param name="timePrecision"></param>
		/// <param name="displayFrameCount"></param>
		/// <returns></returns>
		public static DebugLogItem LogDebugToConsole(object message, string color, int timePrecision, bool displayFrameCount)
		{
			DebugLogItem item = new DebugLogItem(message, color, Time.frameCount, Time.time, timePrecision, displayFrameCount);

			// we add to our DebugLog 
			if (LogHistory.Count > _logHistoryMaxLength)
			{
				LogHistory.RemoveAt(0);
			}

			LogHistory.Add(item);

			// we trigger an event
			MMDebugLogEvent.Trigger(item);

			return item;
		}

		/// <summary>
		/// An event used to broadcast debug logs
		/// </summary>
		public struct MMDebugLogEvent
		{
			static private event Delegate OnEvent;
			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
			static public void Register(Delegate callback) { OnEvent += callback; }
			static public void Unregister(Delegate callback) { OnEvent -= callback; }

			public delegate void Delegate(DebugLogItem item);
			static public void Trigger(DebugLogItem item)
			{
				OnEvent?.Invoke(item);
			}
		}

		#endregion

		#region EnableDisableDebugs

		/// <summary>
		/// whether or not debug logs (MMDebug.DebugLogTime, MMDebug.DebugOnScreen) should be displayed
		/// </summary>
		public static bool DebugLogsEnabled
		{
			get
			{
				if (_debugLogEnabledSet)
				{
					return _debugLogEnabled;
				}
                
				if (PlayerPrefs.HasKey(_editorPrefsDebugLogs))
				{
					_debugLogEnabled = (PlayerPrefs.GetInt(_editorPrefsDebugLogs) == 0) ? false : true;
				}
				else
				{
					_debugLogEnabled = true;
				}

				_debugLogEnabledSet = true;
				return _debugLogEnabled;
			}
			private set
			{
				_debugLogEnabledSet = true;
				_debugLogEnabled = value;
			}
		}

		/// <summary>
		/// whether or not debug draws should be executed
		/// </summary>
		public static bool DebugDrawEnabled
		{
			get
			{
				#if UNITY_EDITOR
				if (_debugDrawEnabledSet)
				{
					return _debugDrawEnabled;
				}

				if (PlayerPrefs.HasKey(_editorPrefsDebugDraws))
				{
					_debugDrawEnabled = (PlayerPrefs.GetInt(_editorPrefsDebugDraws) == 0) ? false : true;
				}
				else
				{
					_debugDrawEnabled = true;
				}
				_debugDrawEnabledSet = true;
				return _debugDrawEnabled;
				#else
                    return false;
				#endif
			}
			private set { }
		}

		private const string _editorPrefsDebugLogs = "DebugLogsEnabled";
		private const string _editorPrefsDebugDraws = "DebugDrawsEnabled";

		/// <summary>
		/// Enables or disables debug logs 
		/// </summary>
		/// <param name="status"></param>
		public static void SetDebugLogsEnabled(bool status)
		{
			DebugLogsEnabled = status;
			_debugLogEnabled = status;
			#if UNITY_EDITOR
			int newStatus = status ? 1 : 0;
			PlayerPrefs.SetInt(_editorPrefsDebugLogs, newStatus);
			#endif
		}

		/// <summary>
		/// Enables or disables debug draws
		/// </summary>
		/// <param name="status"></param>
		public static void SetDebugDrawEnabled(bool status)
		{
			DebugDrawEnabled = status;
			_debugDrawEnabled = status;
			#if UNITY_EDITOR
			int newStatus = status ? 1 : 0;
			PlayerPrefs.SetInt(_editorPrefsDebugDraws, newStatus);
			#endif
		}

		#endregion

		#region Casts

		/// <summary>
		/// Draws a debug ray in 2D and does the actual raycast
		/// </summary>
		/// <returns>The raycast hit.</returns>
		/// <param name="rayOriginPoint">Ray origin point.</param>
		/// <param name="rayDirection">Ray direction.</param>
		/// <param name="rayDistance">Ray distance.</param>
		/// <param name="mask">Mask.</param>
		/// <param name="debug">If set to <c>true</c> debug.</param>
		/// <param name="color">Color.</param>
		public static RaycastHit2D RayCast(Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false)
		{	
			if (drawGizmo && DebugDrawEnabled) 
			{
				Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
			}
			return Physics2D.Raycast(rayOriginPoint,rayDirection,rayDistance,mask);		
		}

		/// <summary>
		/// Does a boxcast and draws a box gizmo
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="size"></param>
		/// <param name="angle"></param>
		/// <param name="direction"></param>
		/// <param name="length"></param>
		/// <param name="mask"></param>
		/// <param name="color"></param>
		/// <param name="drawGizmo"></param>
		/// <returns></returns>
		public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float length, LayerMask mask, Color color, bool drawGizmo = false)
		{
			if (drawGizmo && DebugDrawEnabled)
			{
				Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

				Vector3[] points = new Vector3[8];

				float halfSizeX = size.x / 2f;
				float halfSizeY = size.y / 2f;

				points[0] = rotation * (origin + (Vector2.left * halfSizeX) + (Vector2.up * halfSizeY)); // top left
				points[1] = rotation * (origin + (Vector2.right * halfSizeX) + (Vector2.up * halfSizeY)); // top right
				points[2] = rotation * (origin + (Vector2.right * halfSizeX) - (Vector2.up * halfSizeY)); // bottom right
				points[3] = rotation * (origin + (Vector2.left * halfSizeX) - (Vector2.up * halfSizeY)); // bottom left
                
				points[4] = rotation * ((origin + Vector2.left * halfSizeX + Vector2.up * halfSizeY) + length * direction); // top left
				points[5] = rotation * ((origin + Vector2.right * halfSizeX + Vector2.up * halfSizeY) + length * direction); // top right
				points[6] = rotation * ((origin + Vector2.right * halfSizeX - Vector2.up * halfSizeY) + length * direction); // bottom right
				points[7] = rotation * ((origin + Vector2.left * halfSizeX - Vector2.up * halfSizeY) + length * direction); // bottom left
                                
				Debug.DrawLine(points[0], points[1], color);
				Debug.DrawLine(points[1], points[2], color);
				Debug.DrawLine(points[2], points[3], color);
				Debug.DrawLine(points[3], points[0], color);

				Debug.DrawLine(points[4], points[5], color);
				Debug.DrawLine(points[5], points[6], color);
				Debug.DrawLine(points[6], points[7], color);
				Debug.DrawLine(points[7], points[4], color);
                
				Debug.DrawLine(points[0], points[4], color);
				Debug.DrawLine(points[1], points[5], color);
				Debug.DrawLine(points[2], points[6], color);
				Debug.DrawLine(points[3], points[7], color);

			}
			return Physics2D.BoxCast(origin, size, angle, direction, length, mask);
		}

		/// <summary>
		/// Draws a debug ray without allocating memory
		/// </summary>
		/// <returns>The ray cast non alloc.</returns>
		/// <param name="array">Array.</param>
		/// <param name="rayOriginPoint">Ray origin point.</param>
		/// <param name="rayDirection">Ray direction.</param>
		/// <param name="rayDistance">Ray distance.</param>
		/// <param name="mask">Mask.</param>
		/// <param name="color">Color.</param>
		/// <param name="drawGizmo">If set to <c>true</c> draw gizmo.</param>
		public static RaycastHit2D MonoRayCastNonAlloc(RaycastHit2D[] array, Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false)
		{	
			if (drawGizmo && DebugDrawEnabled) 
			{
				Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
			}
			if (Physics2D.RaycastNonAlloc(rayOriginPoint, rayDirection, array, rayDistance, mask) > 0)
			{
				return array[0];
			}
			return new RaycastHit2D();        	
		}

		/// <summary>
		/// Draws a debug ray in 3D and does the actual raycast
		/// </summary>
		/// <returns>The raycast hit.</returns>
		/// <param name="rayOriginPoint">Ray origin point.</param>
		/// <param name="rayDirection">Ray direction.</param>
		/// <param name="rayDistance">Ray distance.</param>
		/// <param name="mask">Mask.</param>
		/// <param name="debug">If set to <c>true</c> debug.</param>
		/// <param name="color">Color.</param>
		/// <param name="drawGizmo">If set to <c>true</c> draw gizmo.</param>
		public static RaycastHit Raycast3D(Vector3 rayOriginPoint, Vector3 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			if (drawGizmo && DebugDrawEnabled) 
			{
				Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
			}
			RaycastHit hit;
			Physics.Raycast(rayOriginPoint, rayDirection, out hit, rayDistance, mask, queryTriggerInteraction);	
			return hit;
		}

		#endregion

		#region DebugOnScreen
        
		//public static MMConsole _console;
		public static MMDebugOnScreenConsole _console;
		private const string _debugConsolePrefabPath = "MMDebugOnScreenConsole";
                
		/// <summary>
		/// Instantiates a MMConsole if there isn't one already, and adds the message in parameter to it.
		/// </summary>
		/// <param name="message">Message.</param>
		public static void DebugOnScreen(string message)
		{
			if (!DebugLogsEnabled)
			{
				return;
			}

			InstantiateOnScreenConsole();
			_console.AddMessage(message, "", 30);
		}

		/// <summary>
		/// Instantiates a MMConsole if there isn't one already, and displays the label in bold and its value next to it.
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="value">Value.</param>
		/// <param name="fontSize">The optional font size.</param>
		public static void DebugOnScreen(string label, object value, int fontSize=25)
		{
			if (!DebugLogsEnabled)
			{
				return;
			}

			InstantiateOnScreenConsole(fontSize);
			_console.AddMessage(label, value, fontSize);
		}

		/// <summary>
		/// Instantiates the on screen console if there isn't one already
		/// </summary>
		public static void InstantiateOnScreenConsole(int fontSize=25)
		{
			if (!DebugLogsEnabled)
			{
				return;
			}

			if (_console == null)
			{
				// we try to find one in the scene
				_console = (MMDebugOnScreenConsole) GameObject.FindObjectOfType(typeof(MMDebugOnScreenConsole));
			}

			if (_console == null)
			{	
				// we instantiate the console
				GameObject loaded = UnityEngine.Object.Instantiate(Resources.Load(_debugConsolePrefabPath) as GameObject);
				loaded.name = "MMDebugOnScreenConsole";
				_console = loaded.GetComponent<MMDebugOnScreenConsole>();                
			}
		}

		/// <summary>
		/// Use this method to specify what console to use
		/// </summary>
		/// <param name="newConsole"></param>
		public static void SetOnScreenConsole(MMDebugOnScreenConsole newConsole)
		{
			_console = newConsole;
		}

		#endregion

		#region DebugDraw

		/// <summary>
		/// Draws a gizmo arrow going from the origin position and along the direction Vector3
		/// </summary>
		/// <param name="origin">Origin.</param>
		/// <param name="direction">Direction.</param>
		/// <param name="color">Color.</param>
		public static void DrawGizmoArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 3f, float arrowHeadAngle = 25f)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			Gizmos.color = color;
			Gizmos.DrawRay(origin, direction);
	       
			DrawArrowEnd(true, origin, direction, color, arrowHeadLength, arrowHeadAngle);
		}

		/// <summary>
		/// Draws a debug arrow going from the origin position and along the direction Vector3
		/// </summary>
		/// <param name="origin">Origin.</param>
		/// <param name="direction">Direction.</param>
		/// <param name="color">Color.</param>
		public static void DebugDrawArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 0.2f, float arrowHeadAngle = 35f)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			Debug.DrawRay(origin, direction, color);
	       
			DrawArrowEnd(false,origin,direction,color,arrowHeadLength,arrowHeadAngle);
		}

		/// <summary>
		/// Draws a debug arrow going from the origin position and along the direction Vector3
		/// </summary>
		/// <param name="origin">Origin.</param>
		/// <param name="direction">Direction.</param>
		/// <param name="color">Color.</param>
		/// <param name="arrowLength">Arrow length.</param>
		/// <param name="arrowHeadLength">Arrow head length.</param>
		/// <param name="arrowHeadAngle">Arrow head angle.</param>
		public static void DebugDrawArrow(Vector3 origin, Vector3 direction, Color color, float arrowLength, float arrowHeadLength = 0.20f, float arrowHeadAngle = 35.0f)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			Debug.DrawRay(origin, direction * arrowLength, color);

			DrawArrowEnd(false,origin,direction * arrowLength,color,arrowHeadLength,arrowHeadAngle);
		}

		/// <summary>
		/// Draws a debug cross of the specified size and color at the specified point
		/// </summary>
		/// <param name="spot">Spot.</param>
		/// <param name="crossSize">Cross size.</param>
		/// <param name="color">Color.</param>
		public static void DebugDrawCross (Vector3 spot, float crossSize, Color color)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			Vector3 tempOrigin = Vector3.zero;
			Vector3 tempDirection = Vector3.zero;

			tempOrigin.x = spot.x - crossSize / 2;
			tempOrigin.y = spot.y - crossSize / 2;
			tempOrigin.z = spot.z ;
			tempDirection.x = 1; 
			tempDirection.y = 1;
			tempDirection.z = 0;
			Debug.DrawRay (tempOrigin, tempDirection * crossSize, color);

			tempOrigin.x = spot.x - crossSize / 2;
			tempOrigin.y = spot.y + crossSize / 2;
			tempOrigin.z = spot.z ;
			tempDirection.x = 1; 
			tempDirection.y = -1;
			tempDirection.z = 0;
			Debug.DrawRay (tempOrigin, tempDirection * crossSize, color);
		}

		/// <summary>
		/// Draws the arrow end for DebugDrawArrow
		/// </summary>
		/// <param name="drawGizmos">If set to <c>true</c> draw gizmos.</param>
		/// <param name="arrowEndPosition">Arrow end position.</param>
		/// <param name="direction">Direction.</param>
		/// <param name="color">Color.</param>
		/// <param name="arrowHeadLength">Arrow head length.</param>
		/// <param name="arrowHeadAngle">Arrow head angle.</param>
		private static void DrawArrowEnd (bool drawGizmos, Vector3 arrowEndPosition, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 40.0f)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			if (direction == Vector3.zero)
			{
				return;
			}
			Vector3 right = Quaternion.LookRotation (direction) * Quaternion.Euler (arrowHeadAngle, 0, 0) * Vector3.back;
			Vector3 left = Quaternion.LookRotation (direction) * Quaternion.Euler (-arrowHeadAngle, 0, 0) * Vector3.back;
			Vector3 up = Quaternion.LookRotation (direction) * Quaternion.Euler (0, arrowHeadAngle, 0) * Vector3.back;
			Vector3 down = Quaternion.LookRotation (direction) * Quaternion.Euler (0, -arrowHeadAngle, 0) * Vector3.back;
			if (drawGizmos) 
			{
				Gizmos.color = color;
				Gizmos.DrawRay (arrowEndPosition + direction, right * arrowHeadLength);
				Gizmos.DrawRay (arrowEndPosition + direction, left * arrowHeadLength);
				Gizmos.DrawRay (arrowEndPosition + direction, up * arrowHeadLength);
				Gizmos.DrawRay (arrowEndPosition + direction, down * arrowHeadLength);
			}
			else
			{
				Debug.DrawRay (arrowEndPosition + direction, right * arrowHeadLength, color);
				Debug.DrawRay (arrowEndPosition + direction, left * arrowHeadLength, color);
				Debug.DrawRay (arrowEndPosition + direction, up * arrowHeadLength, color);
				Debug.DrawRay (arrowEndPosition + direction, down * arrowHeadLength, color);
			}
		}

		/// <summary>
		/// Draws handles to materialize the bounds of an object on screen.
		/// </summary>
		/// <param name="bounds">Bounds.</param>
		/// <param name="color">Color.</param>
		public static void DrawHandlesBounds(Bounds bounds, Color color)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			#if UNITY_EDITOR
			Vector3 boundsCenter = bounds.center;
			Vector3 boundsExtents = bounds.extents;
		  
			Vector3 v3FrontTopLeft     = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front top left corner
			Vector3 v3FrontTopRight    = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front top right corner
			Vector3 v3FrontBottomLeft  = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front bottom left corner
			Vector3 v3FrontBottomRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front bottom right corner
			Vector3 v3BackTopLeft      = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back top left corner
			Vector3 v3BackTopRight     = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back top right corner
			Vector3 v3BackBottomLeft   = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back bottom left corner
			Vector3 v3BackBottomRight  = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back bottom right corner


			Handles.color = color;

			Handles.DrawLine (v3FrontTopLeft, v3FrontTopRight);
			Handles.DrawLine (v3FrontTopRight, v3FrontBottomRight);
			Handles.DrawLine (v3FrontBottomRight, v3FrontBottomLeft);
			Handles.DrawLine (v3FrontBottomLeft, v3FrontTopLeft);
		         
			Handles.DrawLine (v3BackTopLeft, v3BackTopRight);
			Handles.DrawLine (v3BackTopRight, v3BackBottomRight);
			Handles.DrawLine (v3BackBottomRight, v3BackBottomLeft);
			Handles.DrawLine (v3BackBottomLeft, v3BackTopLeft);
		         
			Handles.DrawLine (v3FrontTopLeft, v3BackTopLeft);
			Handles.DrawLine (v3FrontTopRight, v3BackTopRight);
			Handles.DrawLine (v3FrontBottomRight, v3BackBottomRight);
			Handles.DrawLine (v3FrontBottomLeft, v3BackBottomLeft);  
			#endif
		}

		/// <summary>
		/// Draws a solid rectangle at the specified position and size, and of the specified colors
		/// </summary>
		/// <param name="position"></param>
		/// <param name="size"></param>
		/// <param name="borderColor"></param>
		/// <param name="solidColor"></param>
		public static void DrawSolidRectangle(Vector3 position, Vector3 size, Color borderColor, Color solidColor)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			#if UNITY_EDITOR

			Vector3 halfSize = size / 2f;

			Vector3[] verts = new Vector3[4];
			verts[0] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
			verts[1] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
			verts[2] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
			verts[3] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
			Handles.DrawSolidRectangleWithOutline(verts, solidColor, borderColor);
            
			#endif
		}
        
		/// <summary>
		/// Draws a gizmo sphere of the specified size and color at a position
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="size">Size.</param>
		/// <param name="color">Color.</param>
		public static void DrawGizmoPoint(Vector3 position, float size, Color color)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}
			Gizmos.color = color;
			Gizmos.DrawWireSphere(position,size);
		}

		/// <summary>
		/// Draws a cube at the specified position, and of the specified color and size
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="color">Color.</param>
		/// <param name="size">Size.</param>
		public static void DrawCube (Vector3 position, Color color, Vector3 size)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			Vector3 halfSize = size / 2f; 

			Vector3[] points = new Vector3 []
			{
				position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
				position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),			
				position + new Vector3(halfSize.x,halfSize.y,-halfSize.z),
				position + new Vector3(-halfSize.x,halfSize.y,-halfSize.z),
				position + new Vector3(-halfSize.x,-halfSize.y,-halfSize.z),
				position + new Vector3(halfSize.x,-halfSize.y,-halfSize.z),
			};

			Debug.DrawLine (points[0], points[1], color ); 
			Debug.DrawLine (points[1], points[2], color ); 
			Debug.DrawLine (points[2], points[3], color ); 
			Debug.DrawLine (points[3], points[0], color ); 
		}

		/// <summary>
		/// Draws a cube at the specified position, offset, and of the specified size
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="offset"></param>
		/// <param name="cubeSize"></param>
		/// <param name="wireOnly"></param>
		public static void DrawGizmoCube(Transform transform, Vector3 offset, Vector3 cubeSize, bool wireOnly)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
			Gizmos.matrix = rotationMatrix;
			if (wireOnly)
			{
				Gizmos.DrawWireCube(offset, cubeSize);
			}
			else
			{
				Gizmos.DrawCube(offset, cubeSize);
			}
		}

		/// <summary>
		/// Draws a gizmo rectangle
		/// </summary>
		/// <param name="center">Center.</param>
		/// <param name="size">Size.</param>
		/// <param name="color">Color.</param>
		public static void DrawGizmoRectangle(Vector2 center, Vector2 size, Color color)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			Gizmos.color = color;

			Vector3 v3TopLeft = new Vector3(center.x - size.x/2, center.y + size.y/2, 0);
			Vector3 v3TopRight = new Vector3(center.x + size.x/2, center.y + size.y/2, 0);;
			Vector3 v3BottomRight = new Vector3(center.x + size.x/2, center.y - size.y/2, 0);;
			Vector3 v3BottomLeft = new Vector3(center.x - size.x/2, center.y - size.y/2, 0);;

			Gizmos.DrawLine(v3TopLeft,v3TopRight);
			Gizmos.DrawLine(v3TopRight,v3BottomRight);
			Gizmos.DrawLine(v3BottomRight,v3BottomLeft);
			Gizmos.DrawLine(v3BottomLeft,v3TopLeft);
		}

		/// <summary>
		/// Draws a gizmo rectangle
		/// </summary>
		/// <param name="center">Center.</param>
		/// <param name="size">Size.</param>
		/// <param name="color">Color.</param>
		public static void DrawGizmoRectangle(Vector2 center, Vector2 size, Matrix4x4 rotationMatrix, Color color)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			GL.PushMatrix();

			Gizmos.color = color;

			Vector3 v3TopLeft = rotationMatrix * new Vector3(center.x - size.x / 2, center.y + size.y / 2, 0);
			Vector3 v3TopRight = rotationMatrix * new Vector3(center.x + size.x / 2, center.y + size.y / 2, 0); ;
			Vector3 v3BottomRight = rotationMatrix * new Vector3(center.x + size.x / 2, center.y - size.y / 2, 0); ;
			Vector3 v3BottomLeft = rotationMatrix * new Vector3(center.x - size.x / 2, center.y - size.y / 2, 0); ;

            
			Gizmos.DrawLine(v3TopLeft, v3TopRight);
			Gizmos.DrawLine(v3TopRight, v3BottomRight);
			Gizmos.DrawLine(v3BottomRight, v3BottomLeft);
			Gizmos.DrawLine(v3BottomLeft, v3TopLeft);
			GL.PopMatrix();
		}

		/// <summary>
		/// Draws a rectangle based on a Rect and color
		/// </summary>
		/// <param name="rectangle">Rectangle.</param>
		/// <param name="color">Color.</param>
		public static void DrawRectangle (Rect rectangle, Color color)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			Vector3 pos = new Vector3( rectangle.x + rectangle.width/2, rectangle.y + rectangle.height/2, 0.0f );
			Vector3 scale = new Vector3 (rectangle.width, rectangle.height, 0.0f );

			MMDebug.DrawRectangle (pos, color, scale); 
		}	

		/// <summary>
		/// Draws a rectangle of the specified color and size at the specified position
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="color">Color.</param>
		/// <param name="size">Size.</param>
		public static void DrawRectangle  (Vector3 position, Color color, Vector3 size)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			Vector3 halfSize = size / 2f; 

			Vector3[] points = new Vector3 []
			{
				position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
				position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),	
			};

			Debug.DrawLine (points[0], points[1], color ); 
			Debug.DrawLine (points[1], points[2], color ); 
			Debug.DrawLine (points[2], points[3], color ); 
			Debug.DrawLine (points[3], points[0], color ); 
		}
        
		/// <summary>
		/// Draws a point of the specified color and size at the specified position
		/// </summary>
		/// <param name="pos">Position.</param>
		/// <param name="col">Col.</param>
		/// <param name="scale">Scale.</param>
		public static void DrawPoint (Vector3 position, Color color, float size)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			Vector3[] points = new Vector3[] 
			{
				position + (Vector3.up * size), 
				position - (Vector3.up * size), 
				position + (Vector3.right * size), 
				position - (Vector3.right * size), 
				position + (Vector3.forward * size), 
				position - (Vector3.forward * size)
			}; 		

			Debug.DrawLine (points[0], points[1], color ); 
			Debug.DrawLine (points[2], points[3], color ); 
			Debug.DrawLine (points[4], points[5], color ); 
			Debug.DrawLine (points[0], points[2], color ); 
			Debug.DrawLine (points[0], points[3], color ); 
			Debug.DrawLine (points[0], points[4], color ); 
			Debug.DrawLine (points[0], points[5], color ); 
			Debug.DrawLine (points[1], points[2], color ); 
			Debug.DrawLine (points[1], points[3], color ); 
			Debug.DrawLine (points[1], points[4], color ); 
			Debug.DrawLine (points[1], points[5], color ); 
			Debug.DrawLine (points[4], points[2], color ); 
			Debug.DrawLine (points[4], points[3], color ); 
			Debug.DrawLine (points[5], points[2], color ); 
			Debug.DrawLine (points[5], points[3], color ); 
		}
        
		/// <summary>
		/// Draws a line of the specified color and size using gizmos
		/// </summary>
		/// <param name="position"></param>
		/// <param name="color"></param>
		/// <param name="size"></param>
		public static void DrawGizmoPoint (Vector3 position, Color color, float size)
		{
			if (!DebugDrawEnabled)
			{
				return;
			}

			Vector3[] points = new Vector3[] 
			{
				position + (Vector3.up * size), 
				position - (Vector3.up * size), 
				position + (Vector3.right * size), 
				position - (Vector3.right * size), 
				position + (Vector3.forward * size), 
				position - (Vector3.forward * size)
			}; 		

			Gizmos.color = color;
			Gizmos.DrawLine (points[0], points[1]); 
			Gizmos.DrawLine (points[2], points[3]); 
			Gizmos.DrawLine (points[4], points[5]); 
			Gizmos.DrawLine (points[0], points[2]); 
			Gizmos.DrawLine (points[0], points[3]); 
			Gizmos.DrawLine (points[0], points[4]); 
			Gizmos.DrawLine (points[0], points[5]); 
			Gizmos.DrawLine (points[1], points[2]); 
			Gizmos.DrawLine (points[1], points[3]); 
			Gizmos.DrawLine (points[1], points[4]); 
			Gizmos.DrawLine (points[1], points[5]); 
			Gizmos.DrawLine (points[4], points[2]); 
			Gizmos.DrawLine (points[4], points[3]); 
			Gizmos.DrawLine (points[5], points[2]); 
			Gizmos.DrawLine (points[5], points[3]); 
		}

		#endregion

		#region Info

		public static string GetSystemInfo()
		{
			string result = "SYSTEM INFO";

			#if UNITY_IOS
                 result += "\n[iPhone generation]iPhone.generation.ToString()";
			#endif

			#if UNITY_ANDROID
                result += "\n[system info]" + SystemInfo.deviceModel;
			#endif

			result += "\n<color=#FFFFFF>Device Type :</color> " + SystemInfo.deviceType;
			result += "\n<color=#FFFFFF>OS Version :</color> " + SystemInfo.operatingSystem;
			result += "\n<color=#FFFFFF>System Memory Size :</color> " + SystemInfo.systemMemorySize;
			result += "\n<color=#FFFFFF>Graphic Device Name :</color> " + SystemInfo.graphicsDeviceName + " (version " + SystemInfo.graphicsDeviceVersion + ")";
			result += "\n<color=#FFFFFF>Graphic Memory Size :</color> " + SystemInfo.graphicsMemorySize;
			result += "\n<color=#FFFFFF>Graphic Max Texture Size :</color> " + SystemInfo.maxTextureSize;
			result += "\n<color=#FFFFFF>Graphic Shader Level :</color> " + SystemInfo.graphicsShaderLevel;
			result += "\n<color=#FFFFFF>Compute Shader Support :</color> " + SystemInfo.supportsComputeShaders;

			result += "\n<color=#FFFFFF>Processor Count :</color> " + SystemInfo.processorCount;
			result += "\n<color=#FFFFFF>Processor Type :</color> " + SystemInfo.processorType;
			result += "\n<color=#FFFFFF>3D Texture Support :</color> " + SystemInfo.supports3DTextures;
			result += "\n<color=#FFFFFF>Shadow Support :</color> " + SystemInfo.supportsShadows;

			result += "\n<color=#FFFFFF>Platform :</color> " + Application.platform;
			result += "\n<color=#FFFFFF>Screen Size :</color> " + Screen.width + " x " + Screen.height;
			result += "\n<color=#FFFFFF>DPI :</color> " + Screen.dpi;

			return result;
		}

		#endregion
        
		#region Console
        
		public static void ClearConsole()
		{
			Type logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
			if (logEntries != null)
			{
				MethodInfo clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
				if (clearMethod != null)
				{
					clearMethod.Invoke(null, null);    
				}
			}
		}
        
		#endregion
	}
}