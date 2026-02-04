using UnityEngine;
using DWORD = System.UInt32;

//public struct TraceResult
//{
//    Vector3 position;
//    Vector3 normal;
//    DWORD type;
//};

public class TraceResult
{
    public CollideClass cls;
    /// <summary>
    /// координаты точки столновения , Org.y=level in case of groundlevel
    /// </summary>
    public Vector3 org;
    /// <summary>
    /// расстояние до точки столновения
    /// </summary>
    public float dist;
    /// <summary>
    /// нормаль земли в точке столновения не нормализированная
    /// </summary>
    public Vector3 normal;
    /// <summary>
    /// объект, с которым столкнулись (м.б. null)
    /// в исходном коде игры - просто object, переименован для исключения конфликта со служебным именем
    /// </summary>
    public FPO     coll_object;
    /// <summary>
    /// тип земли
    /// </summary>
    public int ground_type;   // тип земли
    public Vector3 Normal(bool norm) { return (norm )? normal.normalized:normal;  }

    public override string ToString()
    {
        return string.Format("TraceResult: CollideClass {0} dist {1} org {2} ground_type {3} object [{4}]", cls, dist, org, ground_type, coll_object == null ? "none":coll_object);
    }
}