using crc32 = System.UInt32;

public class CmdOnDeath : RadioHandlerCommand
{


    public override bool checkMessage(RadioMessage msg)
    {
        if (msg.Code == 0x84DC655E)
        { // OnUnitKill
            crc32 code = Hasher.HshString(msg.CallerCallsign);
            if (code == myCallerName && (!isIntValid(myUnit) || myUnit == msg.CallerIndex))
            {
                IGroupAi grp_ai = (IGroupAi)myAi.Query(IGroupAi.ID);
                Asserts.Assert(grp_ai != null);
                return setStarted(grp_ai.GetLeaderContact() != null);
            }
        }
        return false;
    }

    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descInt(myparams, "Unit", myUnit);
        return myparams;
    }


    // BaseControllerCommand
    public override crc32 getID()
    {
        string buffer = string.Format("{0}", myUnit);
        //wsprintf(buffer, "%d", myUnit);
        //return Crc32.Code(0x028A460C, buffer); // OnDeath
        return Hasher.Code(0x028A460C, buffer);
    }


    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Unit: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Unit: myUnit = p.myInt; break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    int myUnit; const crc32 prm_Unit = 0x83765C92;

    public override bool setDefaults(IQuery tm)
    {
        myAi = mySender.getAi();
        myCallerName = Hasher.HshString(mySender.getInfo().myCallsign);
        myUnit = -1;// INT_NULL());
        return base.setDefaults(tm);
    }


    IAi myAi;
    crc32 myCallerName;
};