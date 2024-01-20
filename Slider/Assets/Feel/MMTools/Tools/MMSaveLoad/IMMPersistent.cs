namespace MoreMountains.Tools
{
	/// <summary>
	/// An interface classes that want to be saved by the MMPersistencyManager need to implement 
	/// </summary>
    public interface IMMPersistent
    {
	    /// <summary>
	    /// Needs to return a unique Guid used to identify this object 
	    /// </summary>
	    /// <returns></returns>
	    string GetGuid();
	    
	    /// <summary>
	    /// Returns a savable string containing the object's data
	    /// </summary>
	    /// <returns></returns>
	    string OnSave();

	    /// <summary>
	    /// Loads the object's data from the passed string and applies it to its properties
	    /// </summary>
	    /// <param name="data"></param>
	    void OnLoad(string data);

	    /// <summary>
	    /// Whether or not this object should be saved
	    /// </summary>
	    /// <returns></returns>
	    bool ShouldBeSaved();
    }
}
