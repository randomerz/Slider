using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootsSpeedIncrease : MonoBehaviour
{
    public void SpeedUp()
    {
        Player.GetInstance().UpdatePlayerSpeed();
        AchievementManager.SetAchievementStat("collectedBoots", 1);
    }
}
