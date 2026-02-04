using UnityEngine;
using crc32 = System.UInt32;

public class CmdOnReach : ReachCheckCommand
{
    public override bool checkVector(Vector3 org)
    {
        float dist = (Hasher.HshString(myType) == 0xADAA9C4E) ? (org - myVector).sqrMagnitude : Mathf.Pow(org.x - myVector.x, 2) + Mathf.Pow(org.z - myVector.z, 2);
        return dist <= Mathf.Pow(myRadius, 2);
    }
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descFloat(myparams, "Radius", myRadius);
        myparams = descVector(myparams, "Vector", myVector);
        myparams = descString(myparams, "Marker", myMarker);
        return myparams;
    }

    public override bool isParsingCorrect()
    {
        bool success = myRadiusSetuped || myVectorSetuped || myMarkerSetuped;
        if (myMarker != null)
        {
            MARKER_DATA data = myDynGroupService.getMarkerData(Hasher.HshString(myMarker));
            if (data != null)
            {
                myVector = new Vector3(data.x, 0, data.z);
                if (!myRadiusSetuped)
                    myRadius = data.Radius;
            }
            else
                success = false;
        }
        return success && base.isParsingCorrect();
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        string buffer = string.Format("{0}{1}{2}{3}", myVector.x, myVector.z, myRadius, myType);
        //return Hasher.Code(0xE8A9A0B8, buffer);
        return Hasher.HshString("OnReach" + buffer);
    }


    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Marker: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, IParamList.OpType.OP_EQU);
            case prm_Radius: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, IParamList.OpType.OP_EQU);
            case prm_Vector: return new IParamList.PInfo(IParamList.VarType.SPT_VECTOR, IParamList.OpType.OP_EQU);
            case prm_Type: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, IParamList.OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Marker: myMarker = p.myString; myMarkerSetuped = true; break;
            case prm_Radius: myRadius = p.myFloat; myRadiusSetuped = true; break;
            //case prm_Vector: myVector = new Vector3(p.myVector[0], p.myVector[1], p.myVector[2]); myVectorSetuped = true; break;
            case prm_Vector: myVector = p.myVector; myVectorSetuped = true; break;
            case prm_Type: myType = p.myString; break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    protected float myRadius; const crc32 prm_Radius = 0xC32F9493;
    protected string myMarker; const crc32 prm_Marker = 0x7A9CDA37;
    protected Vector3 myVector; const crc32 prm_Vector = 0xE33DB292;
    protected string myType; const crc32 prm_Type = 0xD31307E8;

    public override bool setDefaults(IQuery tm)
    {
        if (base.setDefaults(tm))
        {
            myVector = myContext.getPosition();
            myRadius = myDynGroupService.getReachRadius();
            myType = "Cylinder";
            return true;
        }
        return false;
    }

    bool myRadiusSetuped = false;
    bool myMarkerSetuped = false;
    bool myVectorSetuped = false;
};
