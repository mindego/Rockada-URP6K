using crc32 = System.UInt32;
using static IParamList;

public class CmdAttackCourse : BaseCommand
{
    public override bool exec()
    {
        myCraft.setAttackCourse(myAngle, myType);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descFloat (myparams, "Angle",myAngle);
        myparams = descInt (myparams, "Type",myType);
        return myparams;
    }

    // BaseCommand
    public override  PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Angle: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
            case prm_Type: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Angle: myAngle = p.myFloat; break;
            case prm_Type: myType = p.myInt; ; break;
            default: return false;
        }
        return true;
    }

    float myAngle; const crc32 prm_Angle = 0x3EBF95F2;

    int myType; const crc32 prm_Type = 0xD31307E8;

    public override bool setDefaults(IQuery tm)
    {
        myCraft = (ICraftUnit) tm.Query(ICraftUnit.ID);
        myAngle = 0f;
        myType = 0;
        return myCraft!=null;
    }

    ICraftUnit myCraft;
};

