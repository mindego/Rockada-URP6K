using crc32 = System.UInt32;

public class CmdSetSkill : BaseCommand
{
    // BaseCommand
    public override bool exec()
    {
        mySkill.setSkill(CommandsUtils.GetSkillCodeFromName(myName));
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descString(myparams, "Name", myName);
        return myparams;
    }

    public override bool isParsingCorrect()
    {
        return myName!=null;
    }


    string myName; const crc32 prm_Name = 0x01EE2EC7;

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Name: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Name: myName = p.myString; break;
            default: return false;
        }
        return true;
    }

    public override bool setDefaults(IQuery tm)
    {
        mySkill = (ISkillService) tm.Query(ISkillService.ID);
        return mySkill!=null;
    }

    ISkillService mySkill;
};