using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// An attribute to add to static methods to they can be called via the MMDebugMenu's command line
	/// </summary>
	[AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
	public class MMDebugLogCommandAttribute : System.Attribute { }
}