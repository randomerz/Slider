

using System.Collections.Generic;

public static class MGUnitData
{
    public enum Job
    {
        Rock,
        Paper,
        Scissors,
    }

    public enum Side
    {
        Ally,
        Enemy,
        Neutral,
    }

    [System.Serializable]
    public struct Data
    {
        public Job job;
        public Side side;

        public Data(Job job, Side side)
        {
            this.job = job;
            this.side = side;
        }
    }

    //This is basically a type chart lol.
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