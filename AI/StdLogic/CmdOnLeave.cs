using UnityEngine;
using crc32 = System.UInt32;

public class CmdOnLeave : CmdOnReach
{
    public override bool checkVector(Vector3 org)
    {
        return Mathf.Pow(org.x - myVector.x, 2) + Mathf.Pow(org.z - myVector.z, 2) > Mathf.Pow(myRadius, 2);
    }
    // BaseControllerCommand
    public override crc32 getID()
    {
        string buffer = string.Format("{0}{1}{2}", myVector.x, myVector.z, myRadius);
        return Hasher.Code(0x7E7D13F2,buffer); // OnLeave
    }
};