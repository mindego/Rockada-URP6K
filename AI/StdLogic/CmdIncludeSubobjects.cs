using crc32 = System.UInt32;

public class CmdIncludeSubobjects : BaseGroupCommand
{// BaseCommand
    public override bool exec()
    {
        myGroupService.setIncludeSubobjects(myInclude);
        return true;
    }

   public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Include",myInclude);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        return isIntValid(myInclude);
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Include: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Include: myInclude = p.myInt; break;
            default: return false;
        }
        return true;
    }

    int myInclude;  const crc32 prm_Include = 0x40EFAAE8;


    public override bool setDefaults(IQuery tm)
    {
        myInclude = -1;// INT_NULL())
        return base.setDefaults(tm);
    }


};