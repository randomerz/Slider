using UnityEngine;

public class WifiPatchComment : MonoBehaviour
{
    // Note: The bottom two wifi nodes have been combined into one node that will power two diodes.
    // The timed gate also only needs 5 inputs instead of the visual 6 it shows. 
    //
    // This has been done because there is a set-up that will cause both of the bottom nodes to 
    // register as powered (the wifi batteries turn on), but only one of them gets added to the
    // timed gates input list. We tried debugging this but it doesn't seem worth it.
}