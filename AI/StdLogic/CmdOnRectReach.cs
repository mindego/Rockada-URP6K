using UnityEngine;
using crc32 = System.UInt32;

public class CmdOnRectReach : ReachCheckCommand
{
    public override bool checkVector(Vector3 org)
    {
        return org.x >= myVector1.x && org.z <= myVector1.x && org.x <= myVector2.x && org.z >= myVector2.z;
    }
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descVector (myparams,"Vector1",myVector1);
        myparams = descVector (myparams,"Vector2",myVector2);
        return myparams;
    }

    public override bool isParsingCorrect()
    {
        return myVector1Set && myVector2Set && base.isParsingCorrect();
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        string buffer = string.Format("{0}{1}{2}{3}", myVector1.x, myVector1.z, myVector2.x, myVector2.z);
        return Hasher.Code(0xFA23A2FA, buffer);
    }


    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Vector: return new IParamList.PInfo(IParamList.VarType.SPT_VECTOR, IParamList.OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Vector:
                if (myVector1Set == false)
                {
                    //myVector1 = VECTOR(p.myVector[0], p.myVector[1], p.myVector[2]);
                    myVector1 = p.myVector;
                    myVector1Set = true;
                    return true;
                }
                else if (myVector2Set == false)
                {
                    //myVector2 = VECTOR(p.myVector[0], p.myVector[1], p.myVector[2]);
                    myVector2 = p.myVector;
                    myVector2Set = true;
                    return true;
                }
                break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return false;
    }

    const crc32 prm_Vector = 0xE33DB292;

    public override bool setDefaults(IQuery tm)
    {
        myVector1Set = false;
        myVector2Set = false;
        return base.setDefaults(tm);
    }

    protected Vector3 myVector1;
    protected Vector3 myVector2;
    bool myVector1Set;
    bool myVector2Set;
};
