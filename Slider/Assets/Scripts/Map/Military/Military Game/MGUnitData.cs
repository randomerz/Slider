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
    Neutral,
}

public static class MGUnitData
{

    [System.Serializable]
    public struct Data
    {
        public MGJob job;
        public MGSide side;

        public Data(MGJob job, MGSide side)
        {
            this.job = job;
            this.side = side;
        }
    }

    //This is basically a type chart lol.
    private static Dictionary<MGJob, HashSet<MGJob>> dominations = new()
    {
        { MGJob.Rock , new HashSet<MGJob>() { MGJob.Scissors}},
        { MGJob.Paper, new HashSet<MGJob>() { MGJob.Rock}},
        { MGJob.Scissors, new HashSet<MGJob>() { MGJob.Paper}}
    };

    public static bool Dominates(MGJob attacker, MGJob defender)
    {
        return dominations[attacker].Contains(defender);
    }
}