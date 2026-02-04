using static IParamList;
using crc32 = System.UInt32;

public class CmdAvoidTerrain : BaseCraftGroupCommand
{
    public override bool exec()
    {
        myCraftGroupService.avoidTerrain(myMinAlt, myPredTime);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descFloat(myparams, "PredTime", myPredTime);
        myparams = descFloat(myparams, "MinAlt", myMinAlt);
        return myparams;
    }

    // BaseCommand
    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_PredTime: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
            case prm_MinAlt: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_PredTime: myPredTime = p.myFloat; break;
            case prm_MinAlt: myMinAlt = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    float myPredTime;  const crc32 prm_PredTime = 0x6E65B551;
    float myMinAlt;  const crc32 prm_MinAlt = 0x5D58A49B;


    public override  bool setDefaults(IQuery tm)
    {
        myPredTime =  6f;
        myMinAlt = 50f;
        return base.setDefaults(tm);
    }
};
