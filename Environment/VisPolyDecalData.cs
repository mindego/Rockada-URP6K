using UnityEditor;
using UnityEngine;

public class VisPolyDecalData : PolyDecalData,System.IDisposable
{
    public VisPolyDecalData(int _numnodes, uint _draw_script, uint _texture, uint _material)
    {
        numnodes = _numnodes;
        draw_script = _draw_script;
        texture = _texture;
        material = _material;
        nodes = new Vector3[numnodes];
    }
    ~VisPolyDecalData()
    {
        Dispose();
    }
    public void Dispose()
    {
        nodes = null;
    }
    public override string ToString()
    {
        string res = GetType().ToString() + " " + GetHashCode().ToString("X8");
        res += "\n" + "draw_script" + draw_script.ToString("X8");
        res += "\n" + " material " + material.ToString("X8");
        res += "\n" + " texture " + texture.ToString("X8");
        foreach (Vector3 node in nodes)
        {
            res += "\n\t" + node;
        }
        return res;
    }
}
public class PolyDecalData
{
    public int numnodes;
    public Vector3[] nodes;//nodes[].y should be 0
    public uint draw_script;
    public uint texture;
    public uint material;
    public Matrix3f tc_gen = new Matrix3f(); //Matrix3f 
    public Vector3 tc_base;
    public Geometry.Line linear;
};

