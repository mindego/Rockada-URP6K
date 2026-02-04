public class MissionMenuChanged : IMenuChanged
{
    IGroupAi mpGrp;
    StdMissionAi mpMission;
    IRefMem refmem = new RefMem();

    public MissionMenuChanged()
    {
        mpGrp = null;
        mpMission = null;
    }
    public bool Initialize(IGroupAi grp, StdMissionAi miss)
    {
        mpGrp = grp;
        mpMission = miss;
        return true;
    }

    ~MissionMenuChanged() { }

    public virtual void MenuChanged(bool HasNewItem)
    {
        Asserts.AssertBp(mpMission != null);
        mpMission.UpdateMenu(mpGrp, HasNewItem);

    }

    public void AddRef()
    {
        refmem.AddRef();
    }

    public int Release()
    {
        return refmem.Release();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }
}

