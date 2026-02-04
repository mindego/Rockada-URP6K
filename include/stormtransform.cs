using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public static class StormTransform
{
    public static Vector3 Globus(float a, float h)
    {
        //TODO - перенести в отдельный модуль, возможно статическим методом.
        Vector2 sca = Storm.Math.getSinCos(a * Storm.Math.Pi() / 180);
        Vector2 sch = Storm.Math.getSinCos(h * Storm.Math.Pi() / 180);

        //return Vector3f(sca.u * sch.u, sch.v, sca.v * sch.u);
        return new Vector3(sca.x * sch.x, sch.y, sca.y * sch.x);
    }
}
public class Matrix3f
{
    public Vector3[] raw = new Vector3[3];

    public Vector3 this[int i]
    {
        get
        {
            return raw[i];
        }
        set
        {
            raw[i] = value;
        }
    }
    public Matrix3f() { }
    public Matrix3f(Vector3 v0, Vector3 v1)
    {
        raw[0] = v0; raw[1] = v1; raw[2] = Vector3.Cross(v0, v1);
    }
    public Matrix3f(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        raw[0] = v0; raw[1] = v1; raw[2] = v2;
    }

    public Vector3 GetColumn(int i)
    {
        return new Vector3(raw[0][i], raw[1][i], raw[2][i]);
    }
    public void SetColumn(int i, Vector3 v) { raw[0][i] = v.x; raw[1][i] = v.y; raw[2][i] = v.z; }

    //TODO - реализовать как операторы или просто как методы.
    //  operator Vector3*()
    //  operator Vector3* () { return raw; }

    public void Identity()
    {
        raw[0].Set(1, 0, 0);
        raw[1].Set(0, 1, 0);
        raw[2].Set(0, 0, 1);
    }
    public void Zero()
    {
        raw[0].Set(0, 0, 0);
        raw[1].Set(0, 0, 0);
        raw[2].Set(0, 0, 0);
    }

    public void Scale(Vector3 scale)
    {
        throw new System.NotImplementedException();
        //raw[0] *= scale;
        //raw[1] *= scale;
        //raw[2] *= scale;
    }

    /// <summary>
    /// a can be this
    /// </summary>
    public void Multiply(Matrix3f a, Matrix3f b)
    {
        Asserts.AssertBp(this != b);
        //raw[0] = a[0] * b;
        //raw[1] = a[1] * b;
        //raw[2] = a[2] * b;
        raw[0] = Vector3Matrix3fMultiply(a[0], b);
        raw[1] = Vector3Matrix3fMultiply(a[1], b);
        raw[2] = Vector3Matrix3fMultiply(a[2], b);

    }
    //    Matrix3f MAD_TOOLS_API & operator ^= (Matrix3f& a);             // this = arg*this
    //Matrix3f MAD_TOOLS_API & operator *= (Matrix3f& b);            // this = this*arg

    public void MakeRotation(Vector3 axis, float angle)
    {
        float sin, cos; Storm.Math.SinCos(angle, out sin, out cos);

        float mcos = 1.0f - cos;
        Vector3 p = new Vector3(axis.x * axis.x, axis.y * axis.y, axis.z * axis.z) * mcos;
        Vector3 m = new Vector3(axis.y * axis.z, axis.z * axis.x, axis.x * axis.y) * mcos;
        Vector3 n = axis * sin;
        raw[0] = new Vector3(p.x + cos, m.z - n.z, m.y + n.y);
        raw[1] = new Vector3(m.z + n.z, p.y + cos, m.x - n.x);
        raw[2] = new Vector3(m.y - n.y, m.x + n.x, p.z + cos);
    }
    public void MakeRotation(Vector3 axis)
    {
        float angle = axis.magnitude;
        MakeRotation(axis *= 1f / angle, angle);
    }

    public float Determinat()
    {
        return
              raw[0][0] * (raw[1][1] * raw[2][2] - raw[1][2] * raw[2][1]) +
              raw[0][1] * (raw[1][2] * raw[2][0] - raw[1][0] * raw[2][2]) +
              raw[0][2] * (raw[1][0] * raw[2][1] - raw[1][1] * raw[2][0]);
    }

    /// <summary>
    /// if (ortn) Tr == GetReciprocal
    /// </summary>
    /// <param name="dest"></param>
    public void GetTranspose(ref Matrix3f dest)
    {
        Assert.IsTrue(dest != this);
        dest.raw[0] = GetColumn(0);
        dest.raw[1] = GetColumn(1);
        dest.raw[2] = GetColumn(2);
    }

    public bool GetReciprocal(Matrix3f dest)
    {
        throw new System.NotImplementedException();
    } 
    // return false if det < EPS

    public static Vector3 Vector3Matrix3fMultiply(Vector3 v,Matrix3f m)
    {
        return v[0] * m.raw[0] + v[1] * m.raw[1] + v[2] * m.raw[2];
    }

    public static Vector3 Matrix3fVector3Multiply(Matrix3f m, Vector3 v)
    {
        return m.GetColumn(0) * v[0] + m.GetColumn(1) * v[1] + m.GetColumn(2) * v[2];
    }

    internal Matrix3f MultiplyThis(Matrix3f b)
    {
        Multiply(this, b);
        return this;
    }
}