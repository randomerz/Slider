//L: This is mainly for organization, as a way of breaking up the entire behaviour of the NPC script.
internal abstract class NPCContext
{
    protected NPC context;

    public NPCContext(NPC context)
    {
        this.context = context;
    }

    public virtual void Awake() { }

    public virtual void OnEnable() { }
    public virtual void OnDisable() { }

    public virtual void Start() { }
    public virtual void Update() { }
}
