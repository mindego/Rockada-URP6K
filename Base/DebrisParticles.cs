using UnityEditor.Experimental.GraphView;

public class CountParticles : IROEnumer
{
    VisualDebris owner;
    public virtual bool ProcessRoEnumeration(RO r)
    {
        owner.RegisterParticle();
        return true;
    }

    public CountParticles(VisualDebris debr)
    {
        owner = debr;

    }
};

public class AddParticles : IROEnumer
{
    VisualDebris owner;
    public virtual bool ProcessRoEnumeration(RO part)
    {
        owner.AddParticle((PARTICLE_SYSTEM)part);
        return true;
    }

    public AddParticles(VisualDebris debr)
    {
        owner = debr;
    }
};