using crc32 = System.UInt32;

public abstract class NullCommand : IVmCommand, IParamList
{
    public virtual bool isParsingCorrect() { return true; }
    public virtual IParamList getParamList() { return this; }
    public virtual IParamList.PInfo getParamInfo(crc32 param_name) { return new IParamList.PInfo(); }
    public virtual bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name) { return false; }
    public virtual string getName() { return null; }
    public virtual int getType() { return 0; }
    public virtual void onOverride() { }
    public virtual void onCreate() { }
    public virtual string describeParams(ref string myparams) { return myparams; }

    public abstract bool run();
    public void AddRef() { }
    public int RefCount() { return 0; }
    public int Release() { return 0; }
}

