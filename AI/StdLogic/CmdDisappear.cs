using crc32 = System.UInt32;

public class CmdDisappear : BaseGroupCommand
{
    public override bool exec()
    {
        myGroupService.disappear(myBase, myDist, (uint)myUltimate);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descString(myparams, "Base", myBase);
        myparams = descInt(myparams, "Ultimate", myUltimate);
        myparams = descFloat(myparams, "Dist", myDist);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Base: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
            case prm_Ultimate: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            case prm_Dist: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            //case prm_Base: myBase = p.myString.Replace("\"",null); break;
            case prm_Base: myBase = p.myString.Trim('\"'); break;
            case prm_Ultimate: myUltimate = p.myInt; break;
            case prm_Dist: myDist = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    string myBase; const crc32 prm_Base = 0x9F79AEA0;
    int myUltimate; const crc32 prm_Ultimate = 0x394E06A2;
    float myDist; const crc32 prm_Dist = 0xDEB18036;

    public override bool setDefaults(IQuery tm)
    {
        myUltimate = 1;
        myDist = 4000f;
        return base.setDefaults(tm);
    }
};