using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagitechSTile : STile
{
    [Header("MagiTech")]
    public ParticleSystem tileWideLightningParticles;
    public ParticleSystem tileWideSparkleParticles;

    public void OnEnable()
    {
        MagiTechGrid.OnDesyncStartWorld += CheckDesyncStart;
        MagiTechGrid.OnDesyncEndWorld += CheckDesyncEnd;
    }

    public void OnDisable()
    {
        MagiTechGrid.OnDesyncStartWorld -= CheckDesyncStart;
        MagiTechGrid.OnDesyncEndWorld -= CheckDesyncEnd;
    }

    public override Vector3 calculatePosition(int x, int y)
    {
        return new Vector3(x / 3 * MagiTechGrid.Instance.gridOffset + x % 3 * STILE_WIDTH, y * STILE_WIDTH);
    }

    public override Vector3 calculateMovingPosition(float x, float y)
    {
        Vector3 newPos = STILE_WIDTH * new Vector3(x, y);

        if (x >= 3)
            newPos += new Vector3(MagiTechGrid.Instance.gridOffset - 3 * STILE_WIDTH, 0);
        return newPos;
    }

    private void CheckDesyncStart(object sender, MagiTechGrid.OnDesyncArgs e)
    {
        if (e.anchoredTileIslandId == islandId || e.desyncIslandId == islandId)
        {
            SetDesyncParticles(true);
        }
    }

    private void CheckDesyncEnd(object sender, MagiTechGrid.OnDesyncArgs e)
    {
        if (e.anchoredTileIslandId == islandId || e.desyncIslandId == islandId)
        {
            SetDesyncParticles(false);
        }
    }

    private void SetDesyncParticles(bool value)
    {
        if (value)
        {
            tileWideLightningParticles.Play();
            tileWideSparkleParticles.Play();
        }
        else
        {
            tileWideLightningParticles.Stop();
            tileWideSparkleParticles.Stop();
        }
    }
}
