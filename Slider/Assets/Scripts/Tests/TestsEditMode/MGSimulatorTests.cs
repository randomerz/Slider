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
    public void TestSpawnUnits()
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
}
