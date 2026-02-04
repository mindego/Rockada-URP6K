using UnityEngine;
using crc32 = System.UInt32;

public class CmdOnRectLeave : CmdOnRectReach
{
    public override bool checkVector(Vector3 org)
    {
        return !(org.x >= myVector1.x && org.z <= myVector1.x && org.x <= myVector2.x && org.z >= myVector2.z);
    }
    // BaseControllerCommand
    public override crc32 getID()
    {
        string buffer = string.Format("{0}{1}{2}{3}", myVector1.x, myVector1.z, myVector2.x, myVector2.z);

        return Hasher.Code(0x6CF711B0,buffer);
    }
};