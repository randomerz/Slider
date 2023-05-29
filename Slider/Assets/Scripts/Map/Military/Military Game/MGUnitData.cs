using System.Collections.Generic;

public enum MGJob
{
    Rock,
    Paper,
    Scissors,
}

public enum MGSide
{
    Ally,
    Enemy,
}

[System.Serializable]
public struct MGUnitData
{
    public MGJob job;
    public MGSide side;

    public MGUnitData(MGJob job, MGSide side)
    {
        this.job = job;
        this.side = side;
    }

    //This is basically a type chart lol.
    private static Dictionary<MGJob, HashSet<MGJob>> dominations = new()
    {
        { MGJob.Rock , new HashSet<MGJob>() { MGJob.Scissors}},
        { MGJob.Paper, new HashSet<MGJob>() { MGJob.Rock}},
        { MGJob.Scissors, new HashSet<MGJob>() { MGJob.Paper}}
    };

    //Returns > 0 if attacker wins, < 0 if defender wins, and 0 if it is a tie.
    public static int Dominates(MGJob attacker, MGJob defender)
    {
        if (dominations[attacker].Contains(defender))
        {
            return 1;
        } else if (dominations[defender].Contains(attacker))
        {
            return -1;
        } else
        {
            return 0;
        }
    }
}