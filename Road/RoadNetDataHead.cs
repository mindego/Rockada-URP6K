using System.Text;

public struct RoadNetDataHead
{
    public int CMP_Signature;                  // 0x47504D43
    public int links_count;
    public int nodes_count;
    public int visuals_count;
    public int links_size;
    public int nodes_size;
    public int visuals_size;
    public int Size() { return links_size + nodes_size + visuals_size; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("CMP_Signature " + CMP_Signature.ToString("X8"));                  // 0x47504D43
        sb.AppendLine("links_count " + links_count);
        sb.AppendLine("nodes_count " + nodes_count);
        sb.AppendLine("visuals_count " + visuals_count);
        sb.AppendLine("links_size " + links_size);
        sb.AppendLine("nodes_size " + nodes_size);
        sb.AppendLine("visuals_size " + visuals_size);
        return sb.ToString();
    }
}







