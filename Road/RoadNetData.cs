using System;
using System.Text;

public class RoadNetData
{
    public RoadNetDataHead head;
    public int[] LinksI;
    public LinkData[] Links;
    public int[] NodesI;
    public NodeData[] Nodes;
    public int[] VisualsI;
    public VisualData[] Visuals;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(GetType().ToString());
        sb.AppendLine("Nodes: " + Nodes.Length);
        sb.AppendLine("Links: " + Links.Length);
        sb.AppendLine("VisualDatas: " + Visuals.Length);
        return sb.ToString();

    }
    public RoadNetData() { }
    public RoadNetData(RoadNetDataHead _head)
    {
        RoadNetDataInit(_head);
    }

    public void RoadNetDataInit(RoadNetDataHead _head)
    {
        head = _head;
        LinksI = new int[head.links_count];
        Links = new LinkData[head.links_count];
        NodesI = new int[head.nodes_count];
        Nodes = new NodeData[head.nodes_count];
        VisualsI = new int[head.visuals_count];
        Visuals = new VisualData[head.visuals_count];
    }

    public LinkData GetLinks(int x) { return Links[x]; }
    public NodeData GetNodes(int x) { return Nodes[x]; }
    public VisualData GetVisuals(int x) { return Visuals[x]; }
    public byte[] toArray()
    {
        //TODO - корректно преобразовать объект в массив байтов.
        return new byte[0];
    }
    //    int* GetLinksI()      const { return (int*) ((char*)this + sizeof(RoadNetDataHead) ); }
    //    int      &GetLinksI(int x) const { return (int       &) GetLinksI()[x];  }
    //LinkData* GetLinks(int x)  const { return (LinkData  *) ((char*)this + GetLinksI(x)); }


    //  int* GetNodesI()      const { return (int       *) ((char*)this + sizeof(RoadNetDataHead) + LinksIndexSize()); }
    //  int &GetNodesI(int x) const { return (int       &) GetNodesI()[x];  }
    //  NodeData* GetNodes(int x)  const { return (NodeData  *)  ((char*)this + GetNodesI(x));  }


    //  int* GetVisualsI()      { return (int         *) ((char*)this + sizeof(RoadNetDataHead) + LinksIndexSize() + NodesIndexSize());
    //}
    //  int &GetVisualsI(int x)  { return (int         &)  GetVisualsI()[x]; }
    //  VisualData* GetVisuals(int x)  const { return (VisualData  *)  ((char*)this + GetVisualsI(x)); }

    int LinksIndexSize() { return head.links_count * sizeof(int); }
    int NodesIndexSize() { return head.nodes_count * sizeof(int); }
    int VisualsIndexSize() { return head.visuals_count * sizeof(int); }
    //int IndexSize()  { return Marshal.sizeof(RoadNetDataHead)+LinksIndexSize() + NodesIndexSize() + VisualsIndexSize(); }
    //int FullSize()    { return IndexSize() +head.Size(); }
    void Clear() { head.links_count = head.links_size = head.nodes_size = head.nodes_count = head.visuals_count = head.visuals_size = 0; }

    internal int FullSize()
    {
        throw new NotImplementedException();
    }
}







