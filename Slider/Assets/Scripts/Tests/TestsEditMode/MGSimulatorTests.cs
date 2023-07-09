using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
        Assert.AreEqual(new Vector2Int(4, 4), mgSim.BoardDims);
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
        MGUnitData[] spawnData =
        {
            new MGUnitData(MGJob.Rock, MGSide.Ally),
            new MGUnitData(MGJob.Paper, MGSide.Ally),
            new MGUnitData(MGJob.Scissors, MGSide.Ally),
            new MGUnitData(MGJob.Paper, MGSide.Ally),

            new MGUnitData(MGJob.Rock, MGSide.Enemy),
            new MGUnitData(MGJob.Paper, MGSide.Enemy),
            new MGUnitData(MGJob.Scissors, MGSide.Enemy),
            new MGUnitData(MGJob.Paper, MGSide.Enemy),

            new MGUnitData(MGJob.Rock, MGSide.Ally),
            new MGUnitData(MGJob.Paper, MGSide.Ally),
            new MGUnitData(MGJob.Scissors, MGSide.Enemy),
            new MGUnitData(MGJob.Paper, MGSide.Enemy),

            new MGUnitData(MGJob.Rock, MGSide.Ally),
            new MGUnitData(MGJob.Paper, MGSide.Ally),
            new MGUnitData(MGJob.Scissors, MGSide.Enemy),
            new MGUnitData(MGJob.Paper, MGSide.Enemy),
        };

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                mgSim.SpawnUnit(x, y, spawnData[y * 4 + x]);
            }
        }

        Assert.AreEqual(16, mgSim.Units.Count);

        {
            int count = 0;
            int x = 0;
            int y = 0;
            foreach (MGUnit unit in mgSim.Units)
            {
                MGSpace unitSpace = mgSim.GetSpace(x, y);

                Assert.AreEqual(spawnData[count].job, unit.Data.job);
                Assert.AreEqual(spawnData[count].side, unit.Data.side);
                Assert.AreEqual(unitSpace, unit.CurrSpace);

                count++;
                x = count % 4;
                y = count / 4;
            }
        }
    }

    [Test]
    public void TestMoveUnitSingle()
    {
        MGUnit unit = mgSim.SpawnUnit(0, 0, new MGUnitData(MGJob.Rock, MGSide.Ally));

        //Move a single unit a few times
        mgSim.MoveUnit(mgSim.Units[0], 1, 0);
        Assert.AreEqual(mgSim.GetSpace(1, 0), unit.CurrSpace);

        mgSim.MoveUnit(mgSim.Units[0], 0, 1);
        Assert.AreEqual(mgSim.GetSpace(1, 1), unit.CurrSpace);

        mgSim.MoveUnit(mgSim.Units[0], 0, 2);
        Assert.AreEqual(mgSim.GetSpace(1, 3), unit.CurrSpace);

        mgSim.MoveUnit(mgSim.Units[0], -1, -2);
        Assert.AreEqual(mgSim.GetSpace(0, 1), unit.CurrSpace);
    }

    [Test]
    public void TestMovePlayerUnitOntoEnemyUnitWin()
    {
        MGUnit rockAlly = mgSim.SpawnUnit(0, 0, new MGUnitData(MGJob.Rock, MGSide.Ally));
        MGUnit paperAlly = mgSim.SpawnUnit(0, 1, new MGUnitData(MGJob.Paper, MGSide.Ally));
        MGUnit scissorsAlly = mgSim.SpawnUnit(0, 2, new MGUnitData(MGJob.Scissors, MGSide.Ally));        
        MGUnit scissorsEnemy = mgSim.SpawnUnit(1, 0, new MGUnitData(MGJob.Scissors, MGSide.Enemy));
        MGUnit rockEnemy = mgSim.SpawnUnit(1, 1, new MGUnitData(MGJob.Rock, MGSide.Enemy));
        MGUnit paperEnemy = mgSim.SpawnUnit(1, 2, new MGUnitData(MGJob.Paper, MGSide.Enemy));

        //Player moves all units to the right and all enemies are defeated.
        mgSim.MoveUnit(rockAlly, 1, 0);
        mgSim.MoveUnit(paperAlly, 1, 0);
        mgSim.MoveUnit(scissorsAlly, 1, 0);

        Assert.AreEqual(3, mgSim.Units.Count);  //Ally enemies were destroyed

        Assert.Contains(rockAlly, mgSim.Units);
        Assert.AreEqual(mgSim.GetSpace(1, 0), rockAlly.CurrSpace);

        Assert.Contains(paperAlly, mgSim.Units);
        Assert.AreEqual(mgSim.GetSpace(1, 1), paperAlly.CurrSpace);

        Assert.Contains(scissorsAlly, mgSim.Units);
        Assert.AreEqual(mgSim.GetSpace(1, 2), scissorsAlly.CurrSpace);
    }

    [Test]
    public void TestMovePlayerUnitOntoEnemyUnitLose()
    {
        MGUnit rockAlly = mgSim.SpawnUnit(0, 0, new MGUnitData(MGJob.Rock, MGSide.Ally));
        MGUnit paperAlly = mgSim.SpawnUnit(0, 1, new MGUnitData(MGJob.Paper, MGSide.Ally));
        MGUnit scissorsAlly = mgSim.SpawnUnit(0, 2, new MGUnitData(MGJob.Scissors, MGSide.Ally));
        MGUnit paperEnemy = mgSim.SpawnUnit(1, 0, new MGUnitData(MGJob.Paper, MGSide.Enemy));
        MGUnit scissorsEnemy = mgSim.SpawnUnit(1, 1, new MGUnitData(MGJob.Scissors, MGSide.Enemy));
        MGUnit rockEnemy = mgSim.SpawnUnit(1, 2, new MGUnitData(MGJob.Rock, MGSide.Enemy));

        //Player moves all units to the right, but enemy wins all of them.
        mgSim.MoveUnit(rockAlly, 1, 0);
        mgSim.MoveUnit(paperAlly, 1, 0);
        mgSim.MoveUnit(scissorsAlly, 1, 0);

        Assert.AreEqual(3, mgSim.Units.Count);

        Assert.Contains(paperEnemy, mgSim.Units);
        Assert.AreEqual(mgSim.GetSpace(1, 0), paperEnemy.CurrSpace);
        
        Assert.Contains(scissorsEnemy, mgSim.Units);
        Assert.AreEqual(mgSim.GetSpace(1, 1), scissorsEnemy.CurrSpace);

        Assert.Contains(rockEnemy, mgSim.Units);
        Assert.AreEqual(mgSim.GetSpace(1, 2), rockEnemy.CurrSpace, $"{rockEnemy.CurrSpace.GetPosition()}");
    }

    [Test]
    public void TestMovePlayerUnitOntoEnemyUnitTie()
    {
        MGUnit rockAlly = mgSim.SpawnUnit(0, 0, new MGUnitData(MGJob.Rock, MGSide.Ally));
        MGUnit paperAlly = mgSim.SpawnUnit(0, 1, new MGUnitData(MGJob.Paper, MGSide.Ally));
        MGUnit scissorsAlly = mgSim.SpawnUnit(0, 2, new MGUnitData(MGJob.Scissors, MGSide.Ally));
        MGUnit rockEnemy = mgSim.SpawnUnit(1, 0, new MGUnitData(MGJob.Rock, MGSide.Enemy));
        MGUnit paperEnemy = mgSim.SpawnUnit(1, 1, new MGUnitData(MGJob.Paper, MGSide.Enemy));
        MGUnit scissorsEnemy = mgSim.SpawnUnit(1, 2, new MGUnitData(MGJob.Scissors, MGSide.Enemy));

        //Player moves all units to the right, but enemy wins all of them.
        mgSim.MoveUnit(rockAlly, 1, 0);
        mgSim.MoveUnit(paperAlly, 1, 0);
        mgSim.MoveUnit(scissorsAlly, 1, 0);

        //Everything got destroyed.
        Assert.AreEqual(0, mgSim.Units.Count);
    }

    [Test]
    public void TestMovePlayerUnitOntoOtherPlayerUnit()
    {

    }
}