using DWORD = System.UInt32;

public struct LinkData
{
    public DWORD roaddata;
    //это индексы LinkData в Links, а не NodeData в Nodes
    public int node1;
    public int node2;

    public override string ToString()
    {
        return GetType() + " roaddata " + roaddata.ToString("X8") + " " + node1 + " " + node2;
    }
}







