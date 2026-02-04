using UnityEngine;
using crc32 = System.UInt32;

public abstract class BaseContactCommand : GroupAndPeriodReact
{
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = base.describeParams (ref myparams);
        myparams = descFloat (myparams,"Radius",myRadius);
        myparams = descInt (myparams,"Type",myType);
        return myparams;
    }

    protected bool checkContact(ContactData data)
    {
        if (haveGroup(data.myName))
        {
            float dist;
            if (myType == 0x9D8DC643)
                //dist = VECTOR(data->myDiff.x, 0, data->myDiff.z).Norma2();
                dist = new Vector3(data.myDiff.x, 0, data.myDiff.z).sqrMagnitude;
            else
                dist = data.myDiff.sqrMagnitude;
            return dist <= myRadius * myRadius;
        }
        return false;
    }


    // BaseControllerCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Radius: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int)IParamList.OpType.OP_EQU);
            case prm_Type: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Radius: myRadius = p.myFloat; break;
            case prm_Type: myType = Hasher.HshString(p.myString); break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    protected float myRadius; const crc32 prm_Radius = 0xC32F9493;
    protected uint myType; const crc32 prm_Type = 0xD31307E8;

    public override bool setDefaults(IQuery tm)
    {
        base.setDefaults(tm);
        myType =  0x9D8DC643;
        myInfo = (IContactInfo) tm.Query(IContactInfo.ID);
        return (IContactInfo) tm.Query(IContactInfo.ID) !=null;
    }

    protected IContactInfo myInfo;
};
