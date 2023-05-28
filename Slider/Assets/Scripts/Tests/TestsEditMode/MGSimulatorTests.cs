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
    }

    [Test]
    public void MGSimulatorTestsInit()
    {
        mgSim.Init(new Vector2Int(4, 4));

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                Assert.IsNotNull(mgSim.GetSpace(x, y));
            }
        }
    }
}
