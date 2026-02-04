using geombase;
using UnityEngine;
using DWORD = System.UInt32;

public interface ILaser : IQMemory
{
    public void SetParams(MATRIX s, float _distance, float _start_thickness, float _finish_thickness, FVec4 color);
  public void SetColor(FVec4 color);
};

public class Laser : ILaser
{
    Laser lpme;
    public Laser next;

    Vector3 end;
    MATRIX start;
    public float start_thickness, finish_thickness, distance;
    Sphere m_SphereBound;

    MATRIX m;

    Color laserColor;

    public void InsertIntoChain(ref Laser l)
    {
        next = l;
        if (next!=null) { next.lpme = next; }
        lpme = this;
        l = this;
        //*(lpme = l) = this;
    }

    ~Laser()
    {
        //Assert(*lpme == this);
        lpme = next;
        if (next!=null) { next.lpme = lpme; }
    }
    public void Draw() {
        StormUnityRenderer.DrawLaser(this);
    }

    public void SetParams(MATRIX s, float _distance, float _start_thickness, float _finish_thickness, FVec4 c)
    {
        start = s; end = s.Org + s.Dir * _distance;
        //color = (uint)c.PackARGB();
        SetColor(c);
        start_thickness = _start_thickness;
        finish_thickness = _finish_thickness;

        m = start;
        distance = _distance;

        m_SphereBound = new Sphere(s.Org, new Vector3(_distance, _start_thickness, _finish_thickness).magnitude);
    }
    public void SetColor(FVec4 c)
    {
        //color = (uint)c.PackARGB();
        laserColor = new Color(c.r, c.g, c.b, c.a);
    }

    public Color GetColor()
    {
        return laserColor;
    }

    public Vector3 GetStart()
    {
        return start.Org;
    }

    public Vector3 GetEnd()
    {
        return end;
    }

    public int Release()
    {
        StormUnityRenderer.UnDrawLaser(this);
        return 0;
    }
};