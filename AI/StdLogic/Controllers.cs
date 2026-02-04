using crc32 = System.UInt32;
public static class Controllers
{
    public static IVmController createCountController()
    {
        CountController cmd = new CountController();
        cmd.initialize();
        return cmd;
    }

    public static IVmController createOnceController()
    {
        OnceController cmd = new OnceController();
        cmd.initialize();
        return cmd;
    }

    public static IVmController createInfiniteController()
    {
        InfiniteController cmd = new InfiniteController();
        cmd.initialize();
        return cmd;
    }
}

class OnceController : BaseController
{
    public override bool getResume() { return false; }
};

class InfiniteController : BaseController
{
    public override bool getResume() { return !myShutdowned; }
};

class CountController : BaseController
{
    public override bool getResume()
    {
        return (myCurrentCount > 0) ? --myCurrentCount!=0 : true;
    }

    public override void shutdown()
    {
        myCurrentCount = 1;
    }

    public override void restart()
    {
        myCurrentCount = myCount;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Count: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int) IParamList.OpType.OP_EQU);
        }
        return base.getParamInfo(param_name);
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Count: myCount = p.myInt; break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    // BaseController 
    public override void setDefaults()
    {
        myCount = 1;
    }

    int myCount; const crc32 prm_Count = 0xBBE79499;

    int myCurrentCount;
};
abstract class BaseController : IVmController, IParamList
{


    public virtual void setID(crc32 id)
    {
        if (myID == CRC32.CRC_NULL)
            myID = id;
    }
    public virtual crc32 getID() { return myID; }
    public virtual IParamList getParamList() { return this; }
    public virtual bool isParsingCorrect() { return true; }
    public virtual string getName() { return null; }
    public virtual int getType() { return 0; }
    public virtual void restart() { myShutdowned = false; }
    public virtual void shutdown() { myShutdowned = true; }
    public virtual string describeParams(ref string myparams) { return myparams; }

    public virtual IParamList.PInfo getParamInfo(crc32 param_name) { return new IParamList.PInfo(); }
    public virtual bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name) { return false; }

    public virtual void setDefaults() { }

    public void initialize()
    {
        myID = CRC32.CRC_NULL;
        setDefaults();
    }

    public abstract bool getResume();
    public void AddRef() { }
    public int RefCount() { return 0; }
    public int Release(){ return 0; }

    protected crc32 myID;
    protected bool myShutdowned = false;
}