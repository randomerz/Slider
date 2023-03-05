using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breadge : MonoBehaviour
{
    public void Pickup()
    {
        AchievementManager.IncrementAchievementStat("collectedBreadge", 1);
    }
}
