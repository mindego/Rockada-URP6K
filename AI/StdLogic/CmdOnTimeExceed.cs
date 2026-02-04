using crc32 = System.UInt32;
using static IParamList;

public class CmdOnTimeExceed : BaseControllerCommand
{
    // IVmCommand
    public override bool exec()
    {
        return myTimer.getTime() >= myStartTime;
    }
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descFloat (myparams,"Time",myTime);
        myparams = descFloat (myparams,"Period",myPeriod);
        myparams = descInt (myparams,"Abs",myAbs);
        return myparams;
    }

    public override  bool restart()
    {
        if (myFirstTime)
        {
            myStartTime = myAbs!=0 ? myTime : myTimer.getTime() + myTime;
            myFirstTime = false;
        }
        else
            myStartTime = myTimer.getTime() + myPeriod;
        return base.restart();
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        //char buffer[32];
        //wsprintf(buffer, "%d%d", int(myTime), myAbs);
        string buffer = string.Format("{0}{1}", (int)myTime, myAbs);
        //return Hasher.HshString.Code(0xB5CD309E, buffer); //OnTimeExceed
        return Hasher.HshString("OnTimeExceed"+buffer); //OnTimeExceed
    }


    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Time: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
            case prm_Period: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
            case prm_Abs: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Time: myTime = p.myFloat; break;
            case prm_Period: myPeriod = p.myFloat; break;
            case prm_Abs: myAbs = p.myInt; break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    float myTime; const crc32 prm_Time = 0x3059C884;
    float myPeriod; const crc32 prm_Period = 0x3DEBE407;
    int myAbs; const crc32 prm_Abs = 0xEF21E8B9;

    public override bool setDefaults(IQuery tm)
    {
        myTime =  0f;
        myPeriod=  0f;
        myAbs = 1;
        myFirstTime = true;
        myStartTime = 0f;
        return true;
    }

    bool myFirstTime;
    float myStartTime;
};