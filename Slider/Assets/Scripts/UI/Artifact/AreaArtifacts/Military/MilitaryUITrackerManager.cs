using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryUITrackerManager : UITrackerManager
{
    protected override Vector2 center => new Vector2(19.5f, 19.5f);
    protected override float centerScale => 2; // idk why 2 looks best
}
