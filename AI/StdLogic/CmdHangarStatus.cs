using crc32 = System.UInt32;

public class CmdHangarStatus : BaseCommand
{
    public override bool exec()
    {
        myHangar.setStatus(myLand!=0, myTakeoff!=0);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Land",myLand);
        myparams = descInt (myparams,"Takeoff",myTakeoff);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Land: return new IParamList.PInfo(IParamList.VarType.SPT_INT, IParamList.OpType.OP_EQU);
            case prm_Takeoff: return new IParamList.PInfo(IParamList.VarType.SPT_INT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override  bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Land: myLand = p.myInt; break;
            case prm_Takeoff: myTakeoff = p.myInt; break;
            default: return false;
        }
        return true;
    }

    int myLand; const crc32 prm_Land = 0xF7CD8519;
    int myTakeoff; const crc32 prm_Takeoff = 0x4046F4E6;

    public override bool setDefaults(IQuery tm)
    {
        myHangar = (IHangarUnit)tm.Query(IHangarUnit.ID);
        myLand = 1;
        myTakeoff = 1;
        return myHangar!=null;
    }

    IHangarUnit myHangar;
};
