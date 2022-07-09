using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class NPCCondsController
{
    protected NPC npcContext;

    public NPCCondsController(NPC context)
    {
        npcContext = context;
    }

    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
    public virtual void Update() { }
}
