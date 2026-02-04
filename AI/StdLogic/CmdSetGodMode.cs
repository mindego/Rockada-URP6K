using crc32 = System.UInt32;
using static IParamList;

public class CmdSetGodMode : BaseCoopMissionCommand
{// BaseCommand
    public override bool exec()
    {
        myData.setGodMode((uint)myUnlimitedAmmo, (uint)myUnlimitedArmor, (uint)myNoCollisionsWithGround, (uint)myNoCollisionsWithObject);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt(myparams, "NoCollisionsWithGround", myNoCollisionsWithGround);
        myparams = descInt(myparams, "NoCollisionsWithObject", myNoCollisionsWithObject);
        myparams = descInt(myparams, "UnlimitedAmmo", myUnlimitedAmmo);
        myparams = descInt(myparams, "UnlimitedArmor", myUnlimitedArmor);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        return true;
    }

    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_NoCollisionsWithGround: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
            case prm_NoCollisionsWithObject: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
            case prm_UnlimitedAmmo: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
            case prm_UnlimitedArmor: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_UnlimitedAmmo: myUnlimitedAmmo = p.myInt; break;
            case prm_UnlimitedArmor: myUnlimitedArmor = p.myInt; break;
            case prm_NoCollisionsWithGround: myNoCollisionsWithGround = p.myInt; break;
            case prm_NoCollisionsWithObject: myNoCollisionsWithObject = p.myInt; break;
            default: return false;
        }
        return true;
    }

    int myUnlimitedAmmo; const crc32 prm_UnlimitedAmmo = 0x72ACC17B;
    int myUnlimitedArmor; const crc32 prm_UnlimitedArmor = 0x71D453AC;
    int myNoCollisionsWithGround; const crc32 prm_NoCollisionsWithGround = 0xA2F14EC5;
    int myNoCollisionsWithObject; const crc32 prm_NoCollisionsWithObject = 0xADA36641;


    public override bool setDefaults(IQuery tm)
    {
        myUnlimitedAmmo = 2;
        myUnlimitedArmor = 2;
        myNoCollisionsWithGround = 2;
        myNoCollisionsWithObject = 2;
        return base.setDefaults(tm);
    }
};