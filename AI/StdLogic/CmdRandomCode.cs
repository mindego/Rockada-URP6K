using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;

public class CmdRandomCode : CmdNotifyAll
{
    // IVmCommand
    public override bool exec()
    {
        Debug.Log("Radiomessaging one of "  + myCodes.Count);
        if (myCodes.Count!=0)
        {
            int num = RandomGenerator.RandN(myCodes.Count);
            Asserts.Assert(num >= 0 && num < myCodes.Count);
            RadioMessage msg = new RadioMessage();
            fillOrg(ref msg, myCodes[num]);
            myCode = myCodes[num];
            Debug.Log("Radiomessaging code " + myCode + " : " + msg);
            //myRadio->sendRadioMessage(myCodes[num], myAi, &msg, true, false);
            myRadio.sendRadioMessage(myCodes[num], myAi, msg, true, false);
        }
        return true;
    }
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = base.describeParams (ref myparams);
        myparams = descStrings (myparams,"Codes",myCodes);
        return myparams;
    }

    public override bool isParsingCorrect()
    {
        return myCodes.Count!=0;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Code: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Code: myCodes.Add(p.myString); break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }
    //ADD_STRINGS(Codes,0xBAADF00D);
    List<string> myCodes = new List<string>(); const crc32 prm_Codes = 0xBAADF00D;
};
