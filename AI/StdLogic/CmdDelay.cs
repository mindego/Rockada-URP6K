using UnityEngine;
using crc32 = System.UInt32;

class CmdDelay : BaseCommand
{
    // BaseCommand
    public override bool exec()
    {
        //Debug.Log(string.Format("Exec() {0} Timer {1} myFinishTime {2}", myCommandName, myTimer.getTime(), myFinishTime));
        return myTimer.getTime() >= myFinishTime;
    }

    public override bool restart()
    {
        myFinishTime = myTimer.getTime() + myTime + RandomGenerator.Rand01() * myDisp;
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descFloat(myparams,"Time",myTime);
        myparams = descFloat(myparams, "Disp", myDisp);
        return myparams;
    }

    // ----------- IParamList ------------
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Time: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int)IParamList.OpType.OP_EQU);
            case prm_Disp: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }
    public override bool addParameter(uint param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Time: myTime = p.myFloat; break;
            case prm_Disp: myDisp = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    float myTime; const crc32 prm_Time = 0x3059C884; 
    float myDisp; const crc32 prm_Disp = 0xD9DC442F;


    //------------------------------------

    public override bool setDefaults(IQuery srv)
    {
        myFinishTime = 0;
        myTime = 0f;
        myDisp = 0f;
        //SetDef(myTime, 0f);
        //SetDef(myDisp, 0f);
        return true;
    }
    float myFinishTime;
};

