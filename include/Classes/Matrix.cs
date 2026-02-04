using System;
using UnityEngine;
/// <summary>
/// Аналог Transform 
/// </summary>
public class MATRIX
{
    public Vector3 Dir;
    public Vector3 Org;
    public Vector3 Right;
    public Vector3 Up;

    public bool invalidated = true;

    public override string ToString()
    {
        return string.Format("Org {0}\nDir {1}\nUp {2}\nRight {3}", Org, Dir, Up, Right);
    }
    public MATRIX(Vector3 org, Vector3 dir, Vector3 up, Vector3 right)
    {
        Org = org;
        Dir = dir;
        Up = up;
        Right = right;
    }

    public MATRIX(ref Vector3 org, ref Vector3 dir, ref Vector3 up, ref Vector3 right)
    {
        Org = org;
        Dir = dir;
        Up = up;
        Right = right;
    }
    public MATRIX(MATRIX World, MATRIX Transform)
    {
        Expand(World, Transform);
    }

    public MATRIX(MATRIX reference) : this(reference.Org, reference.Dir, reference.Up, reference.Right) { }
    public MATRIX(ref MATRIX reference) : this(ref reference.Org, ref reference.Dir, ref reference.Up, ref reference.Right) { }

    public MATRIX() : this(Vector3.zero, Vector3.forward, Vector3.up, Vector3.right) { }

    public MATRIX(Vector3 org) : this(org, Vector3.forward, Vector3.up, Vector3.right) { }

    public void Expand(MATRIX World, MATRIX Transform)
    {
        Org = World.Right * Transform.Org.x + World.Up * Transform.Org.y + World.Dir * Transform.Org.z + World.Org;
        Dir = World.Right * Transform.Dir.x + World.Up * Transform.Dir.y + World.Dir * Transform.Dir.z;
        Up = World.Right * Transform.Up.x + World.Up * Transform.Up.y + World.Dir * Transform.Up.z;
        Right = World.Right * Transform.Right.x + World.Up * Transform.Right.y + World.Dir * Transform.Right.z;
    }
    public void Set(MATRIX m)
    {
        Org = m.Org;
        Dir = m.Dir;
        Up = m.Up;
        Right = m.Right;
    }
    public void Set(Vector3 Org, Vector3 Dir, Vector3 Up)
    {
        this.Org = Org;
        this.Dir = Dir;
        this.Up = Up;
        //Right = Vector3.Cross(Dir, Up) * (-1);
        Right = Vector3.Cross(Up, Dir);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="angle">Угол в радианах</param>
    public void TurnUpPrec(float angle)
    {
        Vector2 cs = Storm.Math.getSinCos(angle);
        Dir = Dir * cs.y + Up * cs.x;
        Up = Vector3.Cross(Dir, Right);

    }
    public void BankLeftPrec(float a) { BankRightPrec(-a); }

    /// <summary>
    /// </summary>
    /// <param name="angle">Угол в радианах</param>
    public void TurnRightPrec(float angle)
    {
        Vector2 cs = Storm.Math.getSinCos(angle);
        Dir = Dir * cs.y + Right * cs.x;
        Right = Vector3.Cross(Up, Dir);
        //Dir = Quaternion.AngleAxis(Storm.Math.RD2GRD(angle), Up) * Dir; //Так тоже можно, но прибавка в скорости невелика.
        //Right = Vector3.Cross(Up, Dir);
    }

    public void BankRightPrec(float angle)
    {
        Vector2 cs = Storm.Math.getSinCos(angle);
        Up = Up * cs.y + Right * cs.x;
        Right = Vector3.Cross(Up, Dir) * -1;
    }

    public Vector3 ProjectPoint(Vector3 Local)
    {
        return Right * Local.x + Up * Local.y + Dir * Local.z + Org;
    }

    public void Vectors2Angles(ref float HeadingAngle, ref float PitchAngle, ref float RollAngle)
    {
        PitchAngle = Mathf.Atan2(Dir.y, Mathf.Sqrt(1 - Dir.y * Dir.y));
        //if (!cmpPrecise(fabs(Dir.y), 1, 20))
        if (Mathf.Abs(Dir.y) != 1)
        //if(!cmpPrecise(Mathf.Abs(Dir.y),1,20))
        {
            HeadingAngle = Mathf.Atan2(Dir.x, Dir.z);
            RollAngle = Mathf.Atan2(-Right.y, Up.y);
        }
        else
        {
            HeadingAngle = 0;
            RollAngle = Mathf.Atan2(Right.z, Mathf.Abs(Up.z));
        }
    }
    private bool cmpPrecise(double x, double y, int nDegree)
    {
        if (x == y) return true;
        return (x > y);
    }

    public void Angles2Vectors(float HeadingAngle, float PitchAngle, float RollAngle)
    {
        //Vector2 ha = Storm.Math.getSinCos<double>(HeadingAngle); // D,C
        //Vector2 pa = Storm.Math.getSinCos<double>(PitchAngle); // F,E
        //Vector2 ra = Storm.Math.getSinCos<double>(RollAngle); // B,A

        Vector2 ha = Storm.Math.getSinCos(HeadingAngle); // D,C
        Vector2 pa = Storm.Math.getSinCos(PitchAngle); // F,E
        Vector2 ra = Storm.Math.getSinCos(RollAngle); // B,A

        Dir = new Vector3(ha.x * pa.y, pa.x, ha.y * pa.y);
        Up = new Vector3(-ha.x * pa.x * ra.y + ra.x * ha.y, ra.y * pa.y, -ha.y * pa.x * ra.y - ha.x * ra.x);
        Right = new Vector3(ha.x * pa.x * ra.x + ra.y * ha.y, -ra.x * pa.y, ra.x * ha.y * pa.x - ra.y * ha.x);
    }

    public Vector3 ProjectVector(Vector3 Local)
    {
        return Right * Local.x + Up * Local.y + Dir * Local.z;
    }

    public Vector3 ExpressPoint(Vector3 Local)
    {
        Vector3 Dif = Local - Org;
        
        return new Vector3(Vector3.Dot(Dif, Right), Vector3.Dot(Dif, Up), Vector3.Dot(Dif, Dir));
    }

    public Vector3 ExpressVector(Vector3 Local)
    {
        return new Vector3(Vector3.Dot(Local, Right), Vector3.Dot(Local, Up), Vector3.Dot(Local, Dir));
    }
    public void Rotate(Vector3 Axis, float Angle)
    {
        //MATRIX Turn; Turn.MakeRotation(Axis, Angle);
        //Dir = Turn.ExpressVector(Dir);
        //Up = Turn.ExpressVector(Up);
        //Right = Turn.ExpressVector(Right);
        //            Quaternion currentRotation = Quaternion.LookRotation(Dir, Up);
        //TODO! Доделать-таки! корректный поворот куда надо

        MATRIX Turn = new MATRIX();
        Turn.MakeRotation(Axis, Angle);
        Dir = Turn.ExpressVector(Dir);
        Up = Turn.ExpressVector(Up);
        Right = Turn.ExpressVector(Right);
    }

    public void MakeRotation(Vector3 Axis, float Angle)
    {
        float S, C; Storm.Math.SinCos(Angle * .5f, out S, out C);
        float[] V = new float[] { Axis.x * S, Axis.y * S, Axis.z * S, C };
        float xs = 2 * V[0], ys = 2 * V[1], zs = 2 * V[2];
        float wx = V[3] * xs, wy = V[3] * ys, wz = V[3] * zs;
        float xx = V[0] * xs, xy = V[0] * ys, xz = V[0] * zs;
        float yy = V[1] * ys, yz = V[1] * zs, zz = V[2] * zs;
        Right.Set(1 - (yy + zz), xy - wz, xz + wy);
        Up.Set(xy + wz, 1 - (xx + zz), yz - wx);
        Dir.Set(xz - wy, yz + wx, 1 - (xx + yy));
    }

    public void setOrg(Vector3 org)
    {
        this.Org = org;
    }

    public void InheritSelf(MATRIX Transform)
    {
        Vector3 Dif = Org - Transform.Org;
        //Org.Set(Dif* Transform.Right,Dif* Transform.Up, Dif* Transform.Dir);
        //Dir.Set(Dir* Transform.Right,Dir* Transform.Up, Dir* Transform.Dir);
        //Up.Set(Up* Transform.Right,Up* Transform.Up, Up* Transform.Dir);
        //Right.Set(Right* Transform.Right,Right* Transform.Up, Right* Transform.Dir);

        Org.Set(Vector3.Dot(Dif, Transform.Right), Vector3.Dot(Dif, Transform.Up), Vector3.Dot(Dif, Transform.Dir));
        Dir.Set(Vector3.Dot(Dir, Transform.Right), Vector3.Dot(Dir, Transform.Up), Vector3.Dot(Dir, Transform.Dir));
        Up.Set(Vector3.Dot(Up, Transform.Right), Vector3.Dot(Up, Transform.Up), Vector3.Dot(Up, Transform.Dir));
        Right.Set(Vector3.Dot(Right, Transform.Right), Vector3.Dot(Right, Transform.Up), Vector3.Dot(Right, Transform.Dir));
    }

    public void ExpandSelf(MATRIX World)
    {
        //Vector3 prevOrg = Org;

        Org = World.Right * Org.x + World.Up * Org.y + World.Dir * Org.z + World.Org;
        Dir = World.Right * Dir.x + World.Up * Dir.y + World.Dir * Dir.z;
        Up = World.Right * Up.x + World.Up * Up.y + World.Dir * Up.z;
        Right = World.Right * Right.x + World.Up * Right.y + World.Dir * Right.z;
        //Debug.LogFormat("Expanded {0} to Org: {1}->{2}",this,Org,prevOrg);
    }

    

    public void SetHorizontal(Vector3 dir)
    {
        //TODO Реализовать!
        float du = Vector3.Dot(dir, Vector3.up);
        //if (Mathf.Abs(du) >= atOne.__f)
        if (Mathf.Abs(du) >= GetAtOneF())
        {
            if (du > 0)
            {
                Dir = Vector3.up;
                Right = Vector3.forward;
                Up = Vector3.right;
            }
            else
            {
                Dir = -Vector3.up;
                Right = Vector3.forward;
                Up = -Vector3.right;
            }
        }
        else
        {
            Dir = dir;
            Right = Vector3.Cross(Vector3.up, dir);
            double rl = 1f / Right.magnitude;
            Up = (float)rl * Vector3.Cross(dir, Right);
            Right *= (float)rl;
        }
    }

    private float GetAtOneF()
    {
        int i = 0x3f7ffff0;
        var bytes = BitConverter.GetBytes(i);
        float f = BitConverter.ToSingle(bytes);
        return f;
    }

    internal void Inherit(MATRIX World, MATRIX Transform)
    {
        Vector3 Dif = World.Org - Transform.Org;
        Org.Set(Vector3.Dot(Dif,Transform.Right), Vector3.Dot(Dif , Transform.Up), Vector3.Dot(Dif , Transform.Dir));
        Dir.Set(Vector3.Dot(World.Dir , Transform.Right), Vector3.Dot(World.Dir , Transform.Up), Vector3.Dot(World.Dir , Transform.Dir));
        Up.Set(Vector3.Dot(World.Up , Transform.Right), Vector3.Dot(World.Up , Transform.Up), Vector3.Dot(World.Up , Transform.Dir));
        Right.Set(Vector3.Dot(World.Right , Transform.Right), Vector3.Dot(World.Right , Transform.Up), Vector3.Dot(World.Right , Transform.Dir));
    }
}


