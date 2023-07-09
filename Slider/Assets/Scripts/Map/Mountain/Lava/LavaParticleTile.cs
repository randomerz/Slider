using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class LavaParticleTile : MonoBehaviour
{
    [Serializable]
    public class LavaParticleEmitter
    {
        public ParticleSystem particleSystem;
        public bool dynamic;
    }

    public LavaParticleEmitter[] emitters = new LavaParticleEmitter[4]; //0 1 2 3 = E N W S
}
