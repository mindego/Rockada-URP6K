using crc32 = System.UInt32;

public class CmdPark : BaseDynGroupCommand
{// BaseCommand
    public override bool exec()
    {
        myDynGroupService.park(myBase, myUltimate);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Base",myBase);
        myparams = descInt (myparams,"Ultimate",myUltimate);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        return myBase!=null;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Base: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, IParamList.OpType.OP_EQU);
            case prm_Ultimate: return new IParamList.PInfo(IParamList.VarType.SPT_INT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            //case prm_Base: myBase = p.myString; break;
            case prm_Base: myBase = p.myString.Replace("\"", null) ; break;
            case prm_Ultimate: myUltimate = p.myInt; break;
            default: return false;
        }
        return true;
    }

    string myBase;  const crc32 prm_Base = 0x9F79AEA0;
    int myUltimate; const crc32 prm_Ultimate = 0x394E06A2;

    public override bool setDefaults(IQuery tm)
    {
        if (base.setDefaults(tm))
        {
            myUltimate = 0;
            return true;
        }
        return false;
    }
};