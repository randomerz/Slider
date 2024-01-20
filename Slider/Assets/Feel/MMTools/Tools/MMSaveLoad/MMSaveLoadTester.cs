using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A test object to store data to test the MMSaveLoadManager class
	/// </summary>
	[System.Serializable]
	public class MMSaveLoadTestObject
	{
		public string SavedText;
	}

	/// <summary>
	/// A simple class used in the MMSaveLoadTestScene to test the MMSaveLoadManager class
	/// </summary>
	public class MMSaveLoadTester : MonoBehaviour
	{
		[Header("Bindings")]
		/// the text to save
		[Tooltip("the text to save")]
		public InputField TargetInputField;

		[Header("Save settings")]
		/// the chosen save method (json, encrypted json, binary, encrypted binary)
		[Tooltip("the chosen save method (json, encrypted json, binary, encrypted binary)")]
		public MMSaveLoadManagerMethods SaveLoadMethod = MMSaveLoadManagerMethods.Binary;
		/// the name of the file to save
		[Tooltip("the name of the file to save")]
		public string FileName = "TestObject";
		/// the name of the destination folder
		[Tooltip("the name of the destination folder")]
		public string FolderName = "MMTest/";
		/// the extension to use
		[Tooltip("the extension to use")]
		public string SaveFileExtension = ".testObject";
		/// the key to use to encrypt the file (if needed)
		[Tooltip("the key to use to encrypt the file (if needed)")]
		public string EncryptionKey = "ThisIsTheKey";

		/// Test button
		[MMInspectorButton("Save")]
		public bool TestSaveButton;
		/// Test button
		[MMInspectorButton("Load")]
		public bool TestLoadButton;
		/// Test button
		[MMInspectorButton("Reset")]
		public bool TestResetButton;

		protected IMMSaveLoadManagerMethod _saveLoadManagerMethod;

		/// <summary>
		/// Saves the contents of the TestObject into a file
		/// </summary>
		public virtual void Save()
		{
			InitializeSaveLoadMethod();
			MMSaveLoadTestObject testObject = new MMSaveLoadTestObject();
			testObject.SavedText = TargetInputField.text;
			MMSaveLoadManager.Save(testObject, FileName+SaveFileExtension, FolderName);
		}

		/// <summary>
		/// Loads the saved data
		/// </summary>
		public virtual void Load()
		{
			InitializeSaveLoadMethod();
			MMSaveLoadTestObject testObject = (MMSaveLoadTestObject)MMSaveLoadManager.Load(typeof(MMSaveLoadTestObject), FileName + SaveFileExtension, FolderName);
			TargetInputField.text = testObject.SavedText;
		}

		/// <summary>
		/// Resets all saves by deleting the whole folder
		/// </summary>
		protected virtual void Reset()
		{
			MMSaveLoadManager.DeleteSaveFolder(FolderName);
		}

		/// <summary>
		/// Creates a new MMSaveLoadManagerMethod and passes it to the MMSaveLoadManager
		/// </summary>
		protected virtual void InitializeSaveLoadMethod()
		{
			switch(SaveLoadMethod)
			{
				case MMSaveLoadManagerMethods.Binary:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodBinary();
					break;
				case MMSaveLoadManagerMethods.BinaryEncrypted:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodBinaryEncrypted();
					(_saveLoadManagerMethod as MMSaveLoadManagerEncrypter).Key = EncryptionKey;
					break;
				case MMSaveLoadManagerMethods.Json:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodJson();
					break;
				case MMSaveLoadManagerMethods.JsonEncrypted:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodJsonEncrypted();
					(_saveLoadManagerMethod as MMSaveLoadManagerEncrypter).Key = EncryptionKey;
					break;
			}
			MMSaveLoadManager.SaveLoadMethod = _saveLoadManagerMethod;
		}
	}
}