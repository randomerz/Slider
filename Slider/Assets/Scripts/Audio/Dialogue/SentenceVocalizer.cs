using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentenceVocalizer : MonoBehaviour
{
    private static readonly HashSet<char> endings = new HashSet<char>(new char[]{ '.', '?', '!' });

    public static List<SentenceVocalizer> Parse(string sentence)
    {
        var vocalizers = new List<SentenceVocalizer>();
        string cleaned = sentence.Remove('"').Remove('\'');
        for (int i = 0; i < cleaned.Length; i++)
        {
            char c = cleaned[i];
            if (c == ' ')
            {
                // start new partition
            }
            else if (endings.Contains(c))
            {
                // terminate partition while adding to it
                oaewijafoiwefp
            } else
            {
                // add to partition but not terminate
            }
        }

        return vocalizers;
    }
}
