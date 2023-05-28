using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using MGJob = MGUnitData.Job;
using MGSide = MGUnitData.Side;

public class MGSimulatorTests
{
    MGSimulator mgSim;

    [SetUp]
    public void Init()
    {
        mgSim = new MGSimulator();
        mgSim.Init(new Vector2Int(4, 4));
    }

    [Test]
    public void MGSimulatorTestsInit()
    {
        Assert.AreEqual(mgSim.BoardDims, new Vector2Int(4, 4));
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                Assert.IsNotNull(mgSim.GetSpace(x, y));
            }
        }
        Assert.IsEmpty(mgSim.Units);
    }

    [Test]
    public void TestSpawnUnitsFull()
    {
        MGUnitData.Data[] spawnData =
        {
            new MGUnitData.Data(MGJob.Rock, MGSide.Ally),
            new MGUnitData.Data(MGJob.Paper, MGSide.Ally),
            new MGUnitData.Data(MGJob.Scissors, MGSide.Ally),
            new MGUnitData.Data(MGJob.Paper, MGSide.Ally),

            new MGUnitData.Data(MGJob.Rock, MGSide.Enemy),
            new MGUnitData.Data(MGJob.Paper, MGSide.Enemy),
            new MGUnitData.Data(MGJob.Scissors, MGSide.Enemy),
            new MGUnitData.Data(MGJob.Paper, MGSide.Enemy),

            new MGUnitData.Data(MGJob.Rock, MGSide.Ally),
            new MGUnitData.Data(MGJob.Paper, MGSide.Ally),
            new MGUnitData.Data(MGJob.Scissors, MGSide.Enemy),
            new MGUnitData.Data(MGJob.Paper, MGSide.Enemy),

            new MGUnitData.Data(MGJob.Rock, MGSide.Ally),
            new MGUnitData.Data(MGJob.Paper, MGSide.Ally),
            new MGUnitData.Data(MGJob.Scissors, MGSide.Enemy),
            new MGUnitData.Data(MGJob.Paper, MGSide.Enemy),
        };

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                mgSim.SpawnUnit(x, y, spawnData[y * 4 + x]);
            }
        }

        {
            int count = 0;
            int x = 0;
            int y = 0;
            foreach (MGUnit unit in mgSim.Units)
            {
                MGSpace unitSpace = mgSim.GetSpace(x, y);

                Assert.AreEqual(unit.Data.job, spawnData[count].job);
                Assert.AreEqual(unit.Data.side, spawnData[count].side);
                Assert.AreEqual(unit.CurrSpace, unitSpace);

                count++;
                x = count % 4;
                y = count / 4;
            }
        }
    }

    [Test]
    public void TestMoveSingleUnit()
    {
        mgSim.SpawnUnit(0, 0, new MGUnitData.Data(MGJob.Rock, MGSide.Ally));
        MGUnit unit = mgSim.Units[0];

        //Move a single unit a few times
        mgSim.MoveUnit(mgSim.Units[0], 1, 0);
        Assert.AreEqual(unit.CurrSpace, mgSim.GetSpace(1, 0));

        mgSim.MoveUnit(mgSim.Units[0], 0, 1);
        Assert.AreEqual(unit.CurrSpace, mgSim.GetSpace(1, 1));

        mgSim.MoveUnit(mgSim.Units[0], 0, 2);
        Assert.AreEqual(unit.CurrSpace, mgSim.GetSpace(1, 3));

        mgSim.MoveUnit(mgSim.Units[0], -1, -2);
        Assert.AreEqual(unit.CurrSpace, mgSim.GetSpace(0, 1));
    }
}
