using UnityEngine;
using crc32 = System.UInt32;

public class CmdRouteTo : BaseDynGroupCommand
{// BaseCommand
    public override bool exec()
    {
        myDynGroupService.routeTo(myVector, myTimer.getTime() + myTime, myClear!=0);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descFloat (myparams,"Time",myTime);
        myparams = descInt (myparams,"Clear",myClear);
        myparams = descString (myparams,"Marker",myMarker);
        myparams = descFloat (myparams,"Alt",myAlt);
        myparams = descVector (myparams,"Vector",myVector);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        if (myMarker!=null)
        {
            MARKER_DATA data = myDynGroupService.getMarkerData(Hasher.HshString(myMarker));
            if (data!=null)
            {
                myVector.x = data.x;
                myVector.y = myAlt;
                myVector.z = data.z;
            } else
            {
                Debug.Log("Parsing failed: " + myMarker);
                Debug.Log("Markers count: " + myDynGroupService.GetMarkersCount());
            } 
            return data!=null;
        }
        return true;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Vector: return new IParamList.PInfo(IParamList.VarType.SPT_VECTOR, IParamList.OpType.OP_EQU);
            case prm_Time: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, IParamList.OpType.OP_EQU);
            case prm_Clear: return new IParamList.PInfo(IParamList.VarType.SPT_INT, IParamList.OpType.OP_EQU);
            case prm_Marker: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, IParamList.OpType.OP_EQU);
            case prm_Alt: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            //case prm_Vector: myVector = new Vector3(p.myVector[0], p.myVector[1], p.myVector[2]); break; //TODO Проверить корректность
            case prm_Vector: myVector = p.myVector; break;
            case prm_Time: myTime = p.myFloat; break;
            case prm_Clear: myClear = p.myInt; break;
            case prm_Marker: myMarker = p.myString; break;
            case prm_Alt: myAlt = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    Vector3 myVector; const crc32 prm_Vector = 0xE33DB292;
    float myTime;  const crc32 prm_Time = 0x3059C884;
    int myClear; const crc32 prm_Clear = 0xDB8F21FD;
    string myMarker; const crc32 prm_Marker = 0x7A9CDA37;
    float myAlt; const crc32 prm_Alt = 0xEFC65094;


    public override bool setDefaults(IQuery tm)
    {
        if (base.setDefaults(tm))
        {
            myContext = (IPointExecutionContext) tm.Query(IPointExecutionContext.ID);
            if (myContext!=null)
            {
                Vector3 vect = myContext.getPosition();
                myVector = vect;
                myTime =  0f;
                myClear = 0;
                myAlt = 50f;
            }
            return myContext!=null;
        }
        return false;
    }

    IPointExecutionContext myContext;

};
