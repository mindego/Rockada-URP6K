using UnityEngine;
using crc32 = System.UInt32;

public class CmdSendMessage : CmdNotifyAll
{
    public const uint ID = 0x83E9EFBE;
    // IVmCommand
    public override bool exec()
    {
        float scale = myTimer.getDelta();
        bool radio_free = myRadio.isRadioFree();
        switch (myInfo.update(scale, radio_free))
        {
            case MessageProcessResult.rtCritical:
                send();
                return true;
            case MessageProcessResult.rtSendAndExit:
                send();
                return true;
            case MessageProcessResult.rtExit:
                return true;
            default:
                return false;
        }
    }

    public override object Query(uint cls_id)
    {
        return (cls_id == CmdSendMessage.ID) ? this : base.Query(cls_id);
    }

    public void send()
    {
        RadioMessage rdm = new RadioMessage();
        fillOrg(ref rdm, myCode);
        fillCaller(ref rdm);
        rdm.String1 = myString1;
        rdm.String2 = myString2;
        //Debug.Log(string.Format("Sending radiomessage with: {0} {1} {2}",myCode,myString1,myString2));
        //myRadio.sendRadioMessage(myCode, myAi, &rdm, myToAll, true);
        myRadio.sendRadioMessage(myCode, myAi, rdm, myToAll!=0, true);
    }

    void fillCaller(ref RadioMessage rdm)
    {
        //rdm.VoiceCode = isIntValid(myVoice) ? myVoice : mySender.getVoice();
        rdm.VoiceCode = myVoice!=-1 ? myVoice : mySender.getVoice();
        Debug.Log("mySender.getVoice(): " + rdm.VoiceCode);
        if (myCaller != null)
        {
            if (myCaller[0] == '#')
            {
                rdm.CallerCallsign = null;
                rdm.CallerIndex = 0;
            }
            else
            {
                rdm.CallerCallsign = myCaller;
                rdm.CallerIndex = myCallerIndex;
            }
        }
        else
        {
            CallerInfo info = mySender.getInfo();
            rdm.CallerCallsign = info.myCallsign;
            rdm.CallerIndex = info.myIndex;
        }
        if (myRecipient != null)
        {
            rdm.RecipientCallsign = myRecipient;
            rdm.RecipientIndex = myRecipientIndex;
        }
        else
        {
            rdm.RecipientCallsign = null;
            rdm.RecipientIndex = 0;
        }
    }

    protected void resetTimers()
    {
        //Debug.Log("myInfo " + (myInfo==null ? "Empty":myInfo));
        if (myInfo == null) myInfo = new RadioMessageInfo();
        myInfo.set(myWaitTime, myPostTime, myCritical!=0, myToAll!=0, true);
    }

    public override bool restart()
    {
        base.restart();
        resetTimers();
        return true;
    }

    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = base.describeParams(ref myparams);
        myparams = descString(myparams, "String1", myString1);
        myparams = descString(myparams, "String2", myString2);
        myparams = descFloat(myparams, "WaitTime", myWaitTime);
        myparams = descFloat(myparams, "PostTime", myPostTime);
        myparams = descInt(myparams, "Critical", myCritical);
        myparams = descInt(myparams, "Voice", myVoice);
        myparams = descString(myparams, "Caller", myCaller);
        myparams = descInt(myparams, "CallerIndex", myCallerIndex);
        myparams = descInt(myparams, "CallerIndex", myCallerIndex);
        myparams = descInt(myparams, "CallerIndex", myCallerIndex);
        myparams = descInt(myparams, "ToAll", myToAll);
        return myparams;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_String1: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
            case prm_String2: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
            case prm_WaitTime: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int)IParamList.OpType.OP_EQU);
            case prm_PostTime: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int)IParamList.OpType.OP_EQU);
            case prm_Critical: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            case prm_Voice: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            case prm_Caller: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
            case prm_Recipient: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
            case prm_CallerIndex: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            case prm_RecipientIndex: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            case prm_ToAll: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_String1: myString1 = p.myString; break;
            case prm_String2: myString2 = p.myString; break;
            case prm_WaitTime: myWaitTime = p.myFloat; break;
            case prm_PostTime: myPostTime = p.myFloat; break;
            case prm_Critical: myCritical = p.myInt; break;
            case prm_Voice: myVoice = p.myInt; break;
            case prm_Caller: myCaller = p.myString; break;
            case prm_Recipient: myRecipient = p.myString; break;
            case prm_CallerIndex: myCallerIndex = p.myInt; break;
            case prm_RecipientIndex: myRecipientIndex = p.myInt; break;
            case prm_ToAll: myToAll = p.myInt; break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    //ADD_STRING(String1,   0x1C0A8C2A);
    //ADD_STRING(String2,   0x8503DD90);
    //ADD_FLOAT(WaitTime,  0x25B46DE2);
    //ADD_FLOAT(PostTime,  0xDD033E5B);
    //ADD_INT(Critical,  0x51CC8662);
    //ADD_INT(Voice,     0xD9C588C0);
    //ADD_STRING(Caller        , 0xF51F8D6E);
    //ADD_STRING(Recipient     , 0x150A8615);
    //ADD_INT(ToAll         , 0x6B1F6893);
    //ADD_INT(CallerIndex   , 0xC010E967);
    //ADD_INT(RecipientIndex, 0xDD802DCF);

    string myString1; const crc32 prm_String1 = 0x1C0A8C2A;
    string myString2; const crc32 prm_String2 = 0x8503DD90;
    float myWaitTime; const crc32 prm_WaitTime = 0x25B46DE2;
    float myPostTime; const crc32 prm_PostTime = 0xDD033E5B;
    int myCritical; const crc32 prm_Critical = 0x51CC8662;
    int myVoice; const crc32 prm_Voice = 0xD9C588C0;
    string myCaller; const crc32 prm_Caller = 0xF51F8D6E;
    string myRecipient; const crc32 prm_Recipient = 0x150A8615;
    int myToAll; const crc32 prm_ToAll = 0x6B1F6893;
    int myCallerIndex; const crc32 prm_CallerIndex = 0xC010E967;
    int myRecipientIndex; const crc32 prm_RecipientIndex = 0xDD802DCF;


    public override bool setDefaults(IQuery tm)
    {
        bool ret = base.setDefaults(tm);
        if (ret)
        {
            //SET_DEF(WaitTime, 20.f);
            //SET_DEF(PostTime, 0.25f);
            //SET_DEF(Critical, 0);
            //SET_DEF(Voice, INT_NULL());
            //SET_DEF(ToAll, 0);
            //SET_DEF(CallerIndex, 0);
            //SET_DEF(RecipientIndex, 0);
            myWaitTime = 20f;
            myPostTime = 0.25f;
            myCritical = 0;
            myVoice = -1; //IN_NULL()
            myToAll = 0;
            myCallerIndex = 0;
            myRecipientIndex =  0;
        }
        return ret;
    }

    protected RadioMessageInfo myInfo;
};
