using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This component, on Awake or on demand, will force a SaveLoadMethod on the MMSaveLoadManager, changing the way it saves data to file.
	/// This will impact all classes that use the MMSaveLoadManager (unless they change that method before saving or loading).
	/// If you change the method, your previously existing data files won't be compatible, you'll need to delete them and start with new ones.
	/// </summary>
	public class MMSaveLoadManagerMethod : MonoBehaviour
	{
		[Header("Save and load method")]
		[MMInformation("This component, on Awake or on demand, will force a SaveLoadMethod on the MMSaveLoadManager, changing the way it saves data to file. " +
		               "This will impact all classes that use the MMSaveLoadManager (unless they change that method before saving or loading)." +
		               "If you change the method, your previously existing data files won't be compatible, you'll need to delete them and start with new ones.", 
						MMInformationAttribute.InformationType.Info,false)]

		/// the method to use to save to file
		[Tooltip("the method to use to save to file")]
		public MMSaveLoadManagerMethods SaveLoadMethod = MMSaveLoadManagerMethods.Binary;
		/// the key to use to encrypt the file (if using an encryption method)
		[Tooltip("the key to use to encrypt the file (if using an encryption method)")]
		public string EncryptionKey = "ThisIsTheKey";

		protected IMMSaveLoadManagerMethod _saveLoadManagerMethod;

		/// <summary>
		/// On Awake, we set the MMSaveLoadManager's method to the chosen one
		/// </summary>
		protected virtual void Awake()
		{
			SetSaveLoadMethod();
		}
		
		/// <summary>
		/// Creates a new MMSaveLoadManagerMethod and passes it to the MMSaveLoadManager
		/// </summary>
		public virtual void SetSaveLoadMethod()
		{
			switch(SaveLoadMethod)
			{
				case MMSaveLoadManagerMethods.Binary:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodBinary();
					break;
				case MMSaveLoadManagerMethods.BinaryEncrypted:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodBinaryEncrypted();
					((MMSaveLoadManagerEncrypter)_saveLoadManagerMethod).Key = EncryptionKey;
					break;
				case MMSaveLoadManagerMethods.Json:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodJson();
					break;
				case MMSaveLoadManagerMethods.JsonEncrypted:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodJsonEncrypted();
					((MMSaveLoadManagerEncrypter)_saveLoadManagerMethod).Key = EncryptionKey;
					break;
			}
			MMSaveLoadManager.SaveLoadMethod = _saveLoadManagerMethod;
		}
	}    
}

