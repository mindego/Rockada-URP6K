using crc32 = System.UInt32;

public class CmdUseRoads : BaseTankGroupCommand
{// BaseCommand
    const string nm_UseRoads = "UseRoads";
    public override bool exec()
    {
        myTankGroupService.setUseRoads(myMode);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt(myparams, "Mode", myMode);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        return isIntValid(myMode);
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Mode: return new IParamList.PInfo(IParamList.VarType.SPT_INT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Mode: myMode = p.myInt; break;
        }
        return false;
    }

    int myMode; const crc32 prm_Mode = 0xC807176A;


    public override bool setDefaults(IQuery tm)
    {
        if (base.setDefaults(tm))
        {
            myMode = 0; //TODO - Возможно, правильнее DWORD;
            return true;
        }
        return false;
    }
};

