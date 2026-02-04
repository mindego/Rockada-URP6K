using UnityEngine;

public struct VisualDataHead
{
    public int parent;
    public Vector3 start, end;
    public int vector_count;

    public override string ToString()
    {
        return GetType() + " parent: " + parent + " [" + start + end + "] count" + vector_count;
    }
}







