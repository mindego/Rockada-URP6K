using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

public class CmdOnMessage : RadioHandlerCommand
{
    // IVmCommand
    public override bool checkMessage(RadioMessage msg)
    {
        //if (haveCode(msg.Code))
        //{
            //Debug.Log(string.Format("Message data: {0} haveCode {1} haveCaller {2} haveCallerIndex {3}", msg.Code.ToString("X8"), haveCode(msg.Code), haveCaller(msg.CallerCallsign), haveCallerIndex(msg.CallerIndex)));
            //if (!haveCaller(msg.CallerCallsign))
            //{
                //Debug.Log(string.Format("my caller {0} vs msg caller {1} miss", myCaller, msg.CallerCallsign));
                //Debug.Log(string.Format("my caller empty? [{0}] null? [{1}]", myCaller == string.Empty, myCaller == null));
            //}
        //}
        if (haveCode(msg.Code) && haveCaller(msg.CallerCallsign) && haveCallerIndex(msg.CallerIndex))
        {
            if (myPool != null)
            {
                if (myCopy != 0)
                    myPool.setVariable(Hasher.HshString("CopyOrg"), new IParamList.Param(msg.Org.x, msg.Org.y, msg.Org.z));
                myPool.setVariable(Hasher.HshString("Org"), new IParamList.Param(msg.Org.x, msg.Org.y, msg.Org.z));
                if (msg.CallerCallsign != null)
                    myPool.setVariable(Hasher.HshString("Caller"), new IParamList.Param(msg.CallerCallsign));
                myPool.setVariable(Hasher.HshString("CallerIndex"), new IParamList.Param(msg.CallerIndex));
            }
            //Debug.Log(string.Format("msg caller {1} vs my caller {0} hit", myCaller, msg.CallerCallsign));
            return setStarted(true);
        }
        //Debug.Log(string.Format("msg caller {1} vs my caller {0} miss", myCaller, msg.CallerCallsign));
        return false;
    }
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descStrings(myparams, "Code", myCode);
        myparams = descString(myparams, "Caller", myCaller);
        myparams = descInt(myparams, "CallerIndex", myCallerIndex);
        myparams = descInt(myparams, "Copy", myCopy);
        return myparams;
    }

    public override bool isParsingCorrect()
    {
        return myCode.Count != 0 && base.isParsingCorrect();
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        //TODO что-то здесть не так в подсчёте идентификатора црц32
        crc32 code = 0x39D04393; //OnMessage
        for (int i = 0; i < myCode.Count; ++i)
            code = Hasher.Code(code, myCode[i]);
        if (myCaller != null)
        {
            string buffer = string.Format("{0}{1}", myCaller, myCallerIndex);
            //wsprintf(buffer, "%s%d", cstr(myCaller), myCallerIndex);
            //code = Crc32.Code(code, buffer);
            code = Hasher.Code(code, buffer);
        }

        return code;
    }


    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Code: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
            case prm_Caller: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
            case prm_CallerIndex: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            case prm_Copy: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Code:
                myCode.Add(p.myString);
                myHashCodes.Add(Hasher.HshString(p.myString));
                break;
            //case prm_Caller: myCaller = p.myString; break;
            case prm_Caller: myCaller = p.myString.Replace("\"", null).Replace("\'",null); break;
            case prm_CallerIndex: myCallerIndex = p.myInt; break;
            case prm_Copy: myCopy = p.myInt; break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    List<string> myCode = new List<string>(); const crc32 prm_Code = 0x28D86059;
    string myCaller; const crc32 prm_Caller = 0xF51F8D6E;
    int myCallerIndex; const crc32 prm_CallerIndex = 0xC010E967;
    int myCopy; const crc32 prm_Copy = 0x1277EB43;

    public override bool setDefaults(IQuery tm)
    {
        myCopy = 0;
        myCallerIndex = 0;
        return base.setDefaults(tm);
    }

    bool haveCode(DWORD code)
    {
        for (int i = 0; i < myHashCodes.Count; ++i)
        {
            if (code == myHashCodes[i])
                return true;
        }
        return false;
    }

    bool haveCaller(string caller)
    {
        return myCaller != null ? (Hasher.HshString(myCaller) == Hasher.HshString(caller)) : true;
        //return myCaller != string.Empty ? (Hasher.HshString(myCaller) == Hasher.HshString(caller)) : true;
    }

    bool haveCallerIndex(int index)
    {
        return (myCallerIndex > 0) ? (myCallerIndex == index) : true;
    }

    List<crc32> myHashCodes = new List<crc32>();
};
