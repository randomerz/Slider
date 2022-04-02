using UnityEngine;

class RatBlackboard : TreeBlackboard
{
    #region Singleton
    private static RatBlackboard _instance;

    public static RatBlackboard Instance
    {

        get
        {
            if (_instance == null)
            {
                _instance = new RatBlackboard();
            }

            return _instance;
        }
    }

    private RatBlackboard() { } //Don't allow construction
    #endregion

    public Vector2 destination;
}
