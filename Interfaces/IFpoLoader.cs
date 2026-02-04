public interface IFpoLoader : IRefMem
{
    public Fpo CreateFpo(ObjId id);
    public FPO CreateFPO(ObjId id);
    public void CreateHiddenObjects(FPO f, ICreateHidden ich);
};
