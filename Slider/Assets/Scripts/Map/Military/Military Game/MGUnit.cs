

using System.Collections.Generic;

public static class MGUnits
{
    public enum Job
    {
        Rock,
        Paper,
        Scissors,
    }

    public enum Allegiance
    {
        Ally,
        Enemy,
        Neutral
    }

    private static Dictionary<Job, HashSet<Job>> dominations = new()
    {
        { Job.Rock , new HashSet<Job>() { Job.Scissors}},
        { Job.Paper, new HashSet<Job>() { Job.Rock}},
        { Job.Scissors, new HashSet<Job>() { Job.Paper}}
    };

    public static bool Dominates(Job attacker, Job defender)
    {
        return dominations[attacker].Contains(defender);
    }
}



//public class MGUnit : MGEntity
//{
//}

//public class MGAlliedUnit : MGUnit
//{

//}

//public class MGEnemyUnit : MGUnit
//{

//}

//public class MGSupply : MGEntity
//{

//}