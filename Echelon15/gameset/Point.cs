using UnityEngine;

using uvs = iUnifiedVariableString;
using uvv = iUnifiedVariableVector;
using uvi = iUnifiedVariableInt;
using uvb = iUnifiedVariableBlock;
using uvf = iUnifiedVariableFloat;
using uvr = iUnifiedVariableReference;
using uv = iUnifiedVariable;
using uvc = iUnifiedVariableContainer;
using uva = iUnifiedVariableArray;
using udb = iUnifiedVariableDB;
using tuvc = iUnifiedVariableContainer;
using tuva = iUnifiedVariableArray;
using tudb = iUnifiedVariableDB;
// SequencePoint<Parent,    Implementation, Changer>
public class Point : SequencePoint,IPoint,ILoadableMember
{
    public const uint ID = 0x899B23DF;
    // IObject
    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IStatus.ID: return getIStatus();
            case Point.ID: return this;
            default: return 0;
        }
    }

    public Point(IPointHolderChange ch) : base(ch) { }
    // wrappers for dummy boost
    public override Vector3 getPos()
    {
        return base.getPos();
    }
    public override void setPos(Vector3 pos)
    {
        base.setPos(pos);
    }

    public bool load<UniType>(UniType type, ILoadErrorLog log)
    {
        return base.load((uvv)type, log);
    }

    //public override bool load(uvv gsd, ILoadErrorLog log = null)
    //{
    //    myPos = gsd.GetValue();
    //    return true;
    //}
}