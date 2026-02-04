using crc32 = System.UInt32;

public abstract class GroupAndPeriodReact : BaseControllerCommand
{
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Group",myGroup);
        myparams = descFloat (myparams,"Period",myPeriod);
        return myparams;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Group: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
            case prm_Period: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int)IParamList.OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool restart()
    {
        bool ret = base.restart();
        if (ret && Storm.Math.cmpFloat(myPeriod, 0, 1e-6f))
            myController.shutdown();
        return ret;
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Group: myGroup = p.myString; myHashedName = Hasher.HshString(myGroup); break;
            case prm_Period: myPeriod = p.myFloat; break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    protected string  myGroup;  const crc32 prm_Group = 0x53FE943E;
    protected float myPeriod;  const crc32 prm_Period = 0x3DEBE407;
    public override bool setDefaults(IQuery tm)
    {
        myPeriod =  60f;
        myCheckTime = 0f;
        myHashedName = CRC32.CRC_NULL;
        return true;
    }

    protected float myCheckTime;
    protected void refreshCheckTimer(float time) { myCheckTime = myTimer.getTime() + time; }

    protected bool haveGroup(crc32 name) { return myGroup==null || myHashedName == name; }
    crc32 myHashedName;
};