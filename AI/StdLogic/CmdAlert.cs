using crc32 = System.UInt32;

public class CmdAlert : BaseDynGroupCommand
{// BaseCommand
    public override bool exec()
    {
        IParamList.Param p1;
        IParamList.Param p2;
        if (myPool.getVariable(Hasher.HshString("Org"), out p1))
            //myDynGroupService.alert(new Vector3(p1.myVector[0], p1.myVector[1], p1.myVector[2]));
            myDynGroupService.alert(p1.myVector); //TODO Проверить на корректность
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        return false;
    }
};
