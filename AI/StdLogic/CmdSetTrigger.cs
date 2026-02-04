using crc32 = System.UInt32;
using static IParamList;

public class CmdSetTrigger : BaseCommand
{
    // IVmCommand
    public override bool exec()
    {
        //char buffer[16];
        ///wsprintf(buffer,"Value = %d",myValue);
        //printMessage(buffer,"EXEC");
        myTriggers.setTrigger(myName, myValue);
        return true;
    }
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Name",myName);
       myparams = descInt (myparams,"Value",myValue);
        return myparams;
    }

    public override bool isParsingCorrect()
    {
        if (myName==null)
        {
            if (!isIntValid(myNum))
                return false;
            else
            {
                //char buffer[16];
                //wsprintf(buffer, "T%d", myNum);
                myName = string.Format("T{0}",myNum);
            }
        }
        return true;
    }

    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Name: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            case prm_Value: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
            case prm_Num: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
        }
        return new  PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Name: myName = p.myString; break;
            case prm_Value: myValue = p.myInt; break;
            case prm_Num: myNum = p.myInt; break;
            default: return false;
        }
        return true;
    }

    protected string myName;  const crc32 prm_Name = 0x01EE2EC7;
    protected int myValue;  const crc32 prm_Value = 0x234988CF;
    int myNum;  const crc32 prm_Num = 0x1BF11671;

    public override bool setDefaults(IQuery tm)
    {
        myTriggers = (ITriggerService) tm.Query(ITriggerService.ID);
        myValue = 0;
        myNum = -1;
        return myTriggers!=null;
    }
    protected ITriggerService myTriggers;
};