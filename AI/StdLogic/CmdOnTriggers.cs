using crc32 = System.UInt32;
using static IParamList;
using System.Collections.Generic;

public struct TriggerInfo
{
    public Param myParam;
    public OpType myOperations;
    public string myName;

    public TriggerInfo(crc32 param_name, OpType op, Param p, string real_name)
    {
        myParam = p;
        myOperations = op;
        myName = real_name;
    }
};
public class CmdOnTriggers : BaseControllerCommand
{
    string  descEval(string myparams, string name, OpType op, int val)
    {
        //return myparams+Sprintf(params, "%s%s%d ", name, getOpSign(op), val);
        return myparams + string.Format("{0}{1}{2}",name,getOpSign(op),val);
    }

    // IVmCommand
    public override bool exec()
    {
        bool ret = true;
        for (int i = 0; i < myTriggers.Count; ++i)
        {
            TriggerInfo info = myTriggers[i];
            int value = 0;
            bool finded = myTrigs.getTrigger(info.myName, out value);
            if (finded == false && myErrorReported == false)
            {
                IErrorLog log = mySender.getLog();
                if (log!=null)
                {
                    //char buffer[1024];
                    //descConditions(buffer);
                    //log->printMessage(MSG_ERROR, "Warning : No trigger \"%s\"!", buffer);
                    string buffer;
                    descConditions(out buffer);
                    log.printMessage(MessageCodes.MSG_ERROR, "Warning : No trigger \"{0}\"!", buffer);
                }
                myErrorReported = true;
            }
            if (!evalCondition(value, info.myParam.myInt, info.myOperations))
            {
                ret = false;
                break;
            }
        }
        if (myEnabled)
        {
            if (ret)
            {
                setEnabled(false);
                //printMessage("starting","EXEC");
                return true;
            }
        }
        else if (!ret)
            setEnabled(true);
        return false;
    }

    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        return descConditions (out myparams);
    }

    public override  void onCreate()
    {
        setEnabled(true);
        base.onCreate();
    }

    public override bool isParsingCorrect()
    {
        if (myCountSet == 1)
        {
            base.addParameter(0xBBE79499, OpType.OP_EQU, new Param(1), "Count");
            myCountSet = 0;
        }
        return myTriggers.Count!=0 && base.isParsingCorrect();
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        //char buffer[256];
        //char* cur = buffer;
        string cur = "";
        for (int i = 0; i < myTriggers.Count; ++i)
        {
            TriggerInfo info = myTriggers[i];
            Asserts.Assert(info.myName!=null);
            //cur += wsprintf(cur, "%s%d%d", cstr(info->myName), info->myOperations, info->myParam.myInt);
            cur += string.Format("{0}{1}{2}",info.myName,info.myOperations,info.myParam.myInt);
        }
        //return Crc32.Code(0x75FFA5E0, buffer); ;//OnTriggers
        return Hasher.HshString("OnTriggers" + cur);
    }


    public override PInfo getParamInfo(crc32 param_name)
    {
        PInfo p = base.getParamInfo(param_name);
        if (p.myType == VarType.SPT_NONE)
            p = new PInfo(VarType.SPT_INT, OpType.OP_ALL);
        return p;
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        if (param_name == 0xBBE79499)
            myCountSet = 0;
        if (base.addParameter(param_name, op, p, real_name) == false)
        {
            myTriggers.Add(new TriggerInfo(param_name, op, p, real_name));
        }
        return true;
    }

    public override bool setDefaults(IQuery tm)
    {
        myTrigs = (ITriggerService)tm.Query(ITriggerService.ID);
        myCountSet = 1;
        setEnabled(true);
        myTriggers = new List<TriggerInfo>();
        return myTrigs!=null;
    }

    string descConditions(out string myparams)
    {
        myparams = "";
        for (int i = 0; i < myTriggers.Count; ++i)
        {
            TriggerInfo info = myTriggers[i];
            myparams = descEval (myparams, info.myName, info.myOperations, info.myParam.myInt);
        }
        return myparams;
    }


    protected void setEnabled(bool en)
    {
        //printMessage(en?"enabling":"disabling",st);
        myEnabled = en;
    }

    protected List<TriggerInfo> myTriggers;
    protected ITriggerService myTrigs;
    protected int myCountSet;
    protected bool myEnabled;
    protected bool myErrorReported = false;
};