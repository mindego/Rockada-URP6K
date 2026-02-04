using crc32 = System.UInt32;

public class CmdChangeSide : BaseGroupCommand
{// BaseCommand
    public override bool exec()
    {
        myGroupService.changeSide(mySide);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Side",mySide);
        return myparams;
    }

    // BaseCommand
    public override  bool isParsingCorrect()
    {
        return isIntValid(mySide);
    }

    public override  IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Side: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Side: mySide = p.myInt; break;
            default: return false;
        }
        return true;
    }

    int mySide; const crc32 prm_Side = 0x7C4C4B74;


    public override bool setDefaults(IQuery tm)
    {
        //SET_DEF(Side, INT_NULL());
        mySide = -1;
        return base.setDefaults(tm);
    }


};
