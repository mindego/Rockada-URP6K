using geombase;
using UnityEngine;

public class StormPath : VisualBaseActor
{
    float timer;
    int dim;
    ILaser path;
    public ILaser GetPathApi() { return path; }
    // взаимодействие с окружающей средой
    public override bool Update(float scale)
    {
        timer -= scale;
        return (timer > 0);
    }

    public StormPath(SceneVisualizer _scene,Vector3 start,Vector3 end, FVec4 c, float _lifetime) : base(_scene) 
    {
        timer = _lifetime;

        MATRIX pos = new MATRIX();
        path = pVis.GetSceneApi().CreateLaser();
        if (path!=null)
        {
            pos.Dir = end - start;
            float d = pos.Dir.magnitude;
            pos.Org = start;
            if (d!=0)
            {
                pos.Right = Vector3.Cross(Vector3.up,pos.Dir);
                pos.Up = Vector3.Cross(pos.Dir, pos.Right);
                pos.Dir /= d;
                pos.Right.Normalize();
                pos.Up.Normalize();
            }
            else
            {
                pos.Right = Vector3.right;
                pos.Up = Vector3.up;
                pos.Dir = Vector3.forward;
            }
            path.SetParams(pos, d, 0.3f, 0.3f, c);
        }
    }
    ~StormPath()
    {
        Dispose();
    }
    public override void Dispose()
    {
        if (path!=null)
            path.Release();
    }
};


