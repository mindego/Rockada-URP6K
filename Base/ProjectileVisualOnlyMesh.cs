
/// <summary>
/// ProjectileVisualMeshAndParticle - mesh (Fpo2)
/// </summary>
class ProjectileVisualOnlyMesh :  ProjectileVisual
{
    public const int APPEAR_TICK = 2;
    #region от VisualBaseActor
    public override bool Update(float scale)
    {
        base.Update(scale);
        if (owner!=null)
        {
            tick++;
            if (fpoobject!=null)
            {
                //fpoobject.tm = MathConvert.FromLocus(owner.GetMatrix());
                if (tick == APPEAR_TICK)
                    pVis.AddNonHashObject(fpoobject);
            }
            return true;
        }
        return false;
    }
    #endregion
    // создание/удаление
    private Fpo  fpoobject;
    private int tick;
    public ProjectileVisualOnlyMesh(SceneVisualizer scene,Projectile owner,WPN_DATA wd): base(scene,owner,wd) {
        fpoobject = null;
        tick = 0;
    
        fpoobject= pVis.CreateFPO2(wd.MeshName);
        Asserts.AssertEx(fpoobject != null);
    }
    ~ProjectileVisualOnlyMesh()
    {
        Dispose();
    }
    private bool IsDisposed = false;
    public override void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        if (fpoobject != null)
        {
            if (tick >= APPEAR_TICK)
                pVis.SubNonHashObject(fpoobject);
            fpoobject.Release();
        }
        base.Dispose();
    }
};


