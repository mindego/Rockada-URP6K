using UnityEngine;

public struct NodeDataHead
{
    public Vector3 org;
    public int link_count;

    public override string ToString()
    {
        return GetType() + " " + org + " " + link_count.ToString("X8");
    }
}







