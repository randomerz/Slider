using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class MGSimulatorTestsPlay
{
    MGSimulator mgSim;
    MGUI mGUI;

    [OneTimeSetUp]
    public void LoadScene()
    {
        SceneManager.LoadScene("MilitarySimTest");

    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return new EnterPlayMode();
        yield return new WaitForEndOfFrame();
        mgSim = GameObject.FindObjectOfType<MilitarySimContext>().Simulator;
        mGUI = GameObject.FindObjectOfType<MGUI>();
    }

    [UnityTest]
    public IEnumerator TestSpawnUnitsUITrackers()
    {
        mgSim.SpawnUnit(0, 0, new MGUnitData.Data(MGJob.Rock, MGSide.Ally));
        MGUnit unit = mgSim.Units[0]; 
        yield return null;

        //Make sure only 1 tracker was created.
        Assert.AreEqual(mGUI.GetComponentsInChildren<MGUIUnitTracker>().Length, 1);


        MGUISquare squareWithUnit = mGUI.Squares[0];
        Assert.AreEqual(squareWithUnit.GetPosition(), new Vector2Int(0, 0));
        CheckSquareHasTracker(squareWithUnit);

        mgSim.MoveUnit(unit, 1, 0);
        squareWithUnit = mGUI.Squares[1];
        Assert.AreEqual(squareWithUnit.GetPosition(), new Vector2Int(1, 0));
        CheckSquareHasTracker(squareWithUnit);
    }

    private void CheckSquareHasTracker(MGUISquare square)
    {
        MGUIUnitTracker tracker = square.GetComponentInChildren<MGUIUnitTracker>();
        Assert.IsNotNull(tracker);
    }
}