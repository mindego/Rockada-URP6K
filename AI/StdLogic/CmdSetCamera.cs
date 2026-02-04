using crc32 = System.UInt32;
using static IParamList;
using UnityEngine;

public class CmdSetCamera : BaseCommand
{
    // BaseCommand
    public override bool exec()
    {
        Debug.Log(string.Format("Setting Camera in {0} @ {1} {2} {3} {4} for [{5} {6}]",myMission,myVector,myHeading,myPitch,myRoll,myGroup,myUnit));
        myMission.setCamera(Hasher.HshString(myMode), myVector, myHeading, myPitch, myRoll, myGroup, (uint)myUnit);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Mode",myMode);
        myparams = descVector (myparams,"Vector",myVector);
        myparams = descString (myparams,"Group",myGroup);
        myparams = descInt (myparams,"Unit",myUnit);
        myparams = descFloat (myparams,"Heading",myHeading);
        myparams = descFloat (myparams,"Pitch",myPitch);
        myparams = descFloat (myparams,"Roll",myRoll);
        return myparams;
    }

    string myMode; const crc32 prm_Mode = 0xC807176A;
    Vector3 myVector; const crc32 prm_Vector = 0xE33DB292;
    string myGroup; const crc32 prm_Group = 0x53FE943E;
    int myUnit; const crc32 prm_Unit = 0x83765C92;
    float myHeading; const crc32 prm_Heading = 0x32F6F9A2;
    float myPitch; const crc32 prm_Pitch = 0x19A16E22;
    float myRoll; const crc32 prm_Roll = 0x7178620F;

    public override  PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Mode: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            case prm_Vector: return new PInfo(VarType.SPT_VECTOR, OpType.OP_EQU);
            case prm_Group: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            case prm_Unit: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
            case prm_Heading: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
            case prm_Pitch: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
            case prm_Roll: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Mode: myMode = p.myString.Replace("\"",""); break; // удаляем кавычки
            case prm_Vector: myVector = new Vector3(p.myVector[0], p.myVector[1], p.myVector[2]); break;
            case prm_Group: myGroup = p.myString; break;
            case prm_Unit: myUnit = p.myInt; break;
            case prm_Heading: myHeading = p.myFloat; break;
            case prm_Pitch: myPitch = p.myFloat; break;
            case prm_Roll: myRoll = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    public override bool setDefaults(IQuery tm)
    {
        myMission = (IMissionService)tm.Query(IMissionService.ID);
        myUnit = 0;
        myHeading = 0;
        myPitch = 0;
        myRoll = 0;
        myVector = Vector3.zero;
        return myMission != null;
    }

    IMissionService myMission;
};