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
        yield return new WaitForSeconds(0.1f);
        mgSim = GameObject.FindObjectOfType<MilitarySimContext>().Simulator;
        mGUI = GameObject.FindObjectOfType<MGUI>();
    }

    [UnityTest]
    public IEnumerator TestSpawnUnitsUITrackers()
    {
        MGUnit unit = mgSim.SpawnUnit(0, 0, new MGUnitData(MGJob.Rock, MGSide.Ally)); 
        yield return null;

        //Make sure only 1 tracker was created.
        Assert.AreEqual(1, mGUI.GetComponentsInChildren<MGUIUnitTracker>().Length);


        MGUISquare squareWithUnit = mGUI.GetSquare(0, 0);
        Assert.AreEqual(new Vector2Int(0, 0), squareWithUnit.GetPosition());
        CheckSquareHasTracker(squareWithUnit);

        mgSim.MoveUnit(unit, 1, 0);

        squareWithUnit = mGUI.GetSquare(1, 0);
        Assert.AreEqual(new Vector2Int(1, 0), squareWithUnit.GetPosition());
        CheckSquareHasTracker(squareWithUnit);
    }

    private void CheckSquareHasTracker(MGUISquare square)
    {
        MGUIUnitTracker tracker = square.GetComponentInChildren<MGUIUnitTracker>();
        Assert.IsNotNull(tracker);
    }
}