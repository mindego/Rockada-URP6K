using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Assertions;
using static RoFlags;
using DWORD = System.UInt32;
//public class RO : Storm.Matrix, IHashObject
public abstract class RO : HashableMatrix, IObject
{
    public void AddRef()
    {
        ++RefCount;
    }
    /// <summary>
    /// для реализации конструкций вида DWORD(pFPO) в исходниках "Шторма"
    /// </summary>
    /// <param name="myRO"></param>
    public static explicit operator DWORD(RO myRO)
    {
        if (myRO == null) return Constants.THANDLE_INVALID;
        return (DWORD)myRO.GetHashCode();
    }
    #region
    //Это - странное. Не уверен, что это позволит избежать коллизии хэшей.
    private static DWORD HandleCount = 0;

    private int Handle = 0;
    public override int GetHashCode()
    {
        if (Handle == 0)
        {
            HandleCount++;
            Handle = (int)HandleCount;
        }
        return Handle;
    }
    #endregion

    /// <summary>
    /// Текстовое имя, сохранено для удобства. Код "Шторма" его не сохраняет, конвертируя в crc32
    /// </summary>
    public string TextName { get; set; }


    public override string ToString()
    {
        return TextName + "#" + GetHashCode().ToString("X8");
    }
    //от IHashObject

    // от IHashableMatrix
    public float HashRadius;

    public RO Parent;
    public RO Next, Prev;
    public RO SubObjects;

    public object Link;

    public float MaxRadius;       // radius with subobjects(without all NON_HASH objects)
    //public float SelfRadius;      // radius without subobjects (rough collision)
    public float DebugSelfRadius;
    public float SelfRadius
    {
        get => DebugSelfRadius;
        set
        {
            Debug.LogFormat("SelfRadius set for {0} {1}->{2}", this, DebugSelfRadius, value);
            DebugSelfRadius = value;
        }
    }

    public int CurrentImage;
    public uint Name;

    //int RefCount;
    int DebugRefCount;
    int RefCount
    {
        get { return DebugRefCount; }
        set
        {
            Debug.LogFormat("RefCount for {3} {0} changed {1}->{2}", this, DebugRefCount, value, this.GetType().ToString());
            DebugRefCount = value;
        }
    }
    public Vector3 ToWorldPoint(Vector3 src)
    {
        Vector3 tgt = ProjectPoint(src);
        for (RO o = Parent; o != null; o = o.Parent) tgt = o.ProjectPoint(tgt);
        return tgt;
    }

    public Vector3 ToLocalPoint(Vector3 src)
    {
        return ExpressPoint(Parent != null ? Parent.ToLocalPoint(src) : src);
    }

    public Vector3 ToLocalVector(Vector3 src)
    {
        return ExpressVector(Parent != null ? Parent.ToLocalVector(src) : src);
    }

    public bool myIncludeRecalacRadius = true;
    public RO(uint flags, uint name = 0) : base(flags | HashFlags.OF_GROUP_RENDER)
    {
        //IHashableMatrix(f | OF_GROUP_RENDER)
        //this.flags = flags | HashFlags.OF_GROUP_RENDER;
        //SetFlag(flags | HashFlags.OF_GROUP_RENDER);
        Link = null;
        RefCount = 1;
        Name = name;
    }
    /// <summary>
    ///  Получает RO из подобъектов,чьё имя совпадает с Name
    /// </summary>
    /// <param name="n">Имя (строка) подобъекта</param>
    /// <returns>RO объект или null, если не найден</returns>
    public RO GetSubObject(string n)
    {
        //return GetSubObject((int)Hasher.CodeString(n));
        return GetSubObject(Hasher.CodeString(n));
    }

    public static bool ProjectSphere(out RO_INFO w, MATRIX Cam, float _Aspect, Vector3 v, float r)
    {
        //Debug.Log(string.Format("ProjectSphere Cam: {0} Aspect {1} v{2} r{3}", Cam,_Aspect,v,r));
        w = new RO_INFO();
        Vector3 Dif = v - Cam.Org;
        float dd = Vector3.Dot(Cam.Dir, Dif);
        if (dd < 1) return false;
        dd = _Aspect / dd;
        w.sz = Dif.magnitude;
        w.sr = r * dd;
        w.sx = 0.5f + Vector3.Dot(Cam.Right, Dif) * dd;
        //w.sx = 0.375f + Vector3.Dot(Cam.Right, Dif) * dd;
        //w.sy = 0.375f - Vector3.Dot(Cam.Up, Dif) * dd;
        w.sy = 0.375f - Vector3.Dot(Cam.Up, Dif) * dd;
        return (w.sx > 0 - w.sr && w.sx < 1.00 + w.sr &&
                w.sy > 0 - w.sr && w.sy < 0.75 + w.sr);
    }

    /// <summary>
    /// Получает RO из подобъектов,чьё имя совпадает с Name
    /// </summary>
    /// <param name="Name">Hasher.Codestring от текстового имени</param>
    /// <returns>RO объект или null, если не найден</returns>
    public RO GetSubObject(uint Name)
    {
        for (RO o = SubObjects; o != null; o = o.Next)
        {
            //Debug.Log(string.Format("Matching int {0} vs {1} ({2})", Name.ToString("X8"), o.Name.ToString("X8"), o.TextName));
            if (o.Name == Name) return o;
        }
        Debug.Log("Subobject not found " + Name.ToString("X8") + " hs " + Hasher.StringHsh(Name) + " cde " + Hasher.StringCode(Name));
        return null;
    }

    public RO Top() { RO o = this; while (o.Parent != null) o = o.Parent; return o; }

    public float RecalcRadius()
    {
        MaxRadius = SelfRadius;
        for (RO o = SubObjects; o != null; o = o.Next)
            if (o.GetFlag(RoFlags.ROF_NONHASH_OBJECT) == 0 && o.myIncludeRecalacRadius)
            {
                float r = o.RecalcRadius();
                float dist2 = o.Org.sqrMagnitude;
                if ((r > MaxRadius) || (dist2 > Mathf.Pow(MaxRadius - r, 2)))
                    MaxRadius = Mathf.Sqrt(dist2) + r;
            }
        //Debug.LogFormat("Recalculated Radius for {0} {1}->{2}",this,SelfRadius,MaxRadius);
        return HashRadius = MaxRadius;
    }


    /// <summary>
    /// Прикрепляет объект obj к текущему объекту (this)
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="O">по умолчанию - VZero</param>
    /// <param name="D">по умолчанию - VDir</param>
    /// <param name="U">по умолчанию - VUp</param>
    /// <param name="irr"></param>
    public void AttachObject(RO obj, Vector3 O, Vector3 D, Vector3 U, bool irr = true)
    {
        obj.Org = O;
        obj.Dir = D;
        obj.Up = U;
        obj.Right = Vector3.Cross(U, D);

        obj.Next = SubObjects; obj.Prev = null; obj.Parent = this;
        if (SubObjects != null) SubObjects.Prev = obj;
        SubObjects = obj;

        if (obj.GetFlag(ROF_NONHASH_OBJECT) == 0)
        {
            obj.SetParentFlag(
              (obj.GetFlag(ROF_TANSP | ROF_ST_TANSP) != 0 ? ROF_ST_TANSP : 0) |
              (obj.GetFlag(ROF_SOLID | ROF_ST_SOLID) != 0 ? ROF_ST_SOLID : 0));
            Top().RecalcRadius();
        }

        obj.myIncludeRecalacRadius = irr;
    }

    void SetParentFlag(uint f)
    {
        for (RO r = Parent; r != null; r = r.Parent) r.SetFlag(f);
    }
    public int Release()
    {
        Asserts.Assert(RefCount > 0);
        if ((--RefCount) == 0)
        {
            if (Parent != null) Detach();

            for (RO sub = SubObjects; sub != null; sub = SubObjects)
            {
                sub.Detach();
                sub.Release();
            }

            Destroy();
            Dispose();
            return 0;
        }
        else
        {
            //Debug.LogErrorFormat("Failed to release RO {0}/{1}#{2} RefCount {3}", 
            //    this.TextName, 
            //    this.GetType().ToString(),
            //    GetHashCode().ToString("X8"),
            //    RefCount
            //);
        }
        return RefCount;
    }
    public void MatrixToWorld(ref MATRIX tgt) { for (RO o = this; o != null; o = o.Parent) tgt.ExpandSelf(o); }
    public void MatrixToLocal(ref MATRIX tgt)
    {
        MATRIX Local = new MATRIX(this);
        if (Parent != null) Parent.MatrixToWorld(ref Local);
        tgt.InheritSelf(Local);

    }

    public abstract void Destroy();
    public void ToWorld() { for (RO o = Parent; o != null; o = o.Parent) ExpandSelf(o); }

    public virtual void Detach()
    {
        if (Parent == null) return;
        RO top = Top();

        if (Parent.SubObjects == this)
            Parent.SubObjects = Next;

        if (Next != null) Next.Prev = Prev;
        if (Prev != null) Prev.Next = Next;

        Next = Prev = Parent = null;

        if (GetFlag(ROF_NONHASH_OBJECT) == 0)
        {
            top.ResetFlags();
            top.RecalcRadius();
        }
    }

    private uint ResetFlags()
    {
        ClearFlag(ROF_ST_TANSP | ROF_ST_SOLID);
        for (RO o = SubObjects; o != null; o = o.Next)
            SetFlag(o.ResetFlags());
        return
          (GetFlag(ROF_TANSP | ROF_ST_TANSP) != 0 ? ROF_ST_TANSP : 0) |
          (GetFlag(ROF_SOLID | ROF_ST_SOLID) != 0 ? ROF_ST_SOLID : 0);
    }

    public Vector3 ToWorldVector(Vector3 src)
    {
        Vector3 tgt = ProjectVector(src);
        for (RO o = Parent; o != null; o = o.Parent) tgt = o.ProjectVector(tgt);
        return tgt;
    }

    public void EnumerateSubobjects(IROEnumer e, uint SubobjClass)
    {
        for (RO r = SubObjects; r != null; r = r.Next)
        {
            if (r.GetFlag(SubobjClass) != 0) e.ProcessRoEnumeration(r);
            if (r.SubObjects != null) r.EnumerateSubobjects(e, SubobjClass);
        }
    }
}

public interface IROEnumer
{
    public bool ProcessRoEnumeration(RO r);
};

public interface IROEnumer2
{
    public bool ProcessRoEnumeration2(RO r, MATRIX m);
};