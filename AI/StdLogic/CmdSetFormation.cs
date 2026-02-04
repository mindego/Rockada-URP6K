using crc32 = System.UInt32;

public class CmdSetFormation : BaseDynGroupCommand
{// BaseCommand
    public override bool exec()
    {
        myDynGroupService.setFormation(myName, myDist);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Name",myName);
        myparams = descFloat (myparams,"Dist",myDist);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        return myName!=null;
    }
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Name: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, IParamList.OpType.OP_EQU);
            case prm_Dist: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Name: myName = p.myString; break;
            case prm_Dist: myDist = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    string myName; const crc32 prm_Name = 0x01EE2EC7;
    float myDist; const crc32 prm_Dist = 0xDEB18036;

    public override bool setDefaults(IQuery tm)
    {
        myDist = -1f;
        return base.setDefaults(tm);
    }
}
