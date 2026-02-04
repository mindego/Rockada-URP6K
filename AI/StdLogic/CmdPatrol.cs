using UnityEngine;
using static IParamList;
using crc32 = System.UInt32;

public class CmdPatrol : BaseCraftGroupCommand
{
    public override  bool exec()
    {
        //if (myMarker!=null && myVector.NormaFAbs() == 0f) //fabs(x)+fabs(y)+fabs(z), так что проще magnitude померять
        //if (myMarker != null && myVector.sqrMagnitude == 0f)
        if (myMarker != null && Storm.Math.NormaFAbs(myVector) == 0f)
        {
            myGroupService.getLeaderOrg(out myVector);
            myVector.y = myHeight;
        }
        myCraftGroupService.patrol(myVector, myDist);
        return true;
    }

    public override string  describeParams(ref string myparams)
    {
        myparams = descString (myparams, "Marker",myMarker);
        myparams = descFloat (myparams, "Height",myHeight);
        myparams = descFloat (myparams, "Dist",myDist);
        myparams = descVector (myparams, "Vector",myVector);
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
                myVector.y = myHeight;
                myVector.z = data.z;
                if (!myDistSetuped)
                    myDist = data.Radius;
            }
            return data!=null;
        }
        return true;
    }

    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Vector: return new PInfo(VarType.SPT_VECTOR, OpType.OP_EQU);
            case prm_Marker: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            case prm_Height: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
            case prm_Dist: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override  bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Vector: myVector = new Vector3(p.myVector[0], p.myVector[1], p.myVector[2]); break;
            case prm_Marker: myMarker = p.myString; break;
            case prm_Height: myHeight = p.myFloat; break;
            case prm_Dist: myDist = p.myFloat; myDistSetuped = true; break;
            default: return false;
        }
        return true;
    }

    Vector3 myVector;  const crc32 prm_Vector = 0xE33DB292;
    string myMarker;  const crc32 prm_Marker = 0x7A9CDA37;
    float myHeight;  const crc32 prm_Height = 0x0D1E1FC6;
    float myDist;  const crc32 prm_Dist = 0xDEB18036;


    public override bool setDefaults(IQuery tm)
    {
        if (base.setDefaults(tm))
        {
            myVector = Vector3.zero;
            myHeight = 500f;
            myDist = 2000f;
            myDynGroupService = (IDynGroupService) tm.Query(IDynGroupService.ID);
            myGroupService = (IGroupService) tm.Query(IGroupService.ID);
            return myDynGroupService!=null && myGroupService!=null;
        }
        return false;
    }

    IDynGroupService myDynGroupService;
    IGroupService myGroupService;
    bool myDistSetuped = false;
};
