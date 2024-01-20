#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
#endif
using UnityEngine;
using System.Threading.Tasks;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This class is used to automatically install optional dependencies used in MMFeedbacks
	/// </summary>
	public static class MMFDependencyInstaller 
	{
		#if UNITY_EDITOR
		static ListRequest _listRequest;
		static AddRequest _addRequest;
		static int _currentIndex;
        
		private static string[] _packages = new string[] 
		{"com.unity.cinemachine", 
			"com.unity.postprocessing", 
			"com.unity.textmeshpro",
			"com.unity.2d.animation"
		};

		/// <summary>
		/// Installs all dependencies listed in _packages
		/// </summary>
		[MenuItem("Tools/More Mountains/MMFeedbacks/Install All Dependencies", false, 703)]
		public static void InstallAllDependencies()
		{
			_currentIndex = 0;
			_listRequest = null;
			_addRequest = null;
            
			Debug.Log("[MMFDependencyInstaller] Installation start");
			_listRequest = Client.List();    
            
			EditorApplication.update += ListProgress;
		}

		/// <summary>
		/// Installs all dependencies, use this when calling from a running application
		/// </summary>
		public async static void InstallFromPlay()
		{
			EditorApplication.ExitPlaymode();
			while (Application.isPlaying)
			{
				await Task.Delay(500);
			}
			await Task.Delay(500);
            
			ClearConsole();
            
			await Task.Delay(500);
			InstallAllDependencies();
		}
        
		/// <summary>
		/// Clears the console. Why isn't this a built-in one liner? Who knows. 
		/// </summary>
		public static void ClearConsole()
		{
			var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
			if (logEntries != null)
			{
				var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
				if (clearMethod != null)
				{
					clearMethod.Invoke(null, null);    
				}
			}
		}

		/// <summary>
		/// Proceeds to install the next package in line
		/// </summary>
		static void InstallNext()
		{
			if (_currentIndex < _packages.Length)
			{
				bool packageFound = false;
				foreach (var package in _listRequest.Result)
				{
					if (package.name == _packages[_currentIndex])
					{
						packageFound = true;
						Debug.Log("[MMFDependencyInstaller] "+package.name+" is already installed");
						_currentIndex++;
						InstallNext();
						return;
					} 
				}

				if (!packageFound)
				{
					Debug.Log("[MMFDependencyInstaller] installing "+_packages[_currentIndex]);
					_addRequest = Client.Add(_packages[_currentIndex]);
					EditorApplication.update += AddProgress;
				}
			}
			else
			{
				Debug.Log("[MMFDependencyInstaller] Installation complete");
				Debug.Log("[MMFDependencyInstaller] It's recommended to now close that scene and reopen it before playing it.");
			}
		}

		/// <summary>
		/// Processes the list request
		/// </summary>
		static void ListProgress()
		{
			if (_listRequest.IsCompleted)
			{
				EditorApplication.update -= ListProgress;
				if (_listRequest.Status == StatusCode.Success)
				{
					InstallNext();
				}
				else if (_listRequest.Status >= StatusCode.Failure)
				{
					Debug.Log(_listRequest.Error.message);
				}
			}
		}
        
		/// <summary>
		/// Processes add requests
		/// </summary>
		static void AddProgress()
		{
			if (_addRequest.IsCompleted)
			{
				if (_addRequest.Status == StatusCode.Success)
				{
					Debug.Log("[MMFDependencyInstaller] "+_addRequest.Result.packageId+" has been installed");
					_currentIndex++;
					InstallNext();
				}
				else if (_addRequest.Status >= StatusCode.Failure)
				{
					Debug.Log(_addRequest.Error.message);
				}
				EditorApplication.update -= AddProgress;
			}
		}
		#endif
	}    
}