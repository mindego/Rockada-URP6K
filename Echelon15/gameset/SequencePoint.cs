//template <class Parent, class Implementation, class Changer>
using UnityEngine;
using uvv = iUnifiedVariableVector;

public class PointImp : iPointImp
{
    public bool save(uvv gsd)
    {
        gsd.SetValue(myPos);
        //saveNotify();
        return true;
    }

    public bool load(uvv gsd, ILoadErrorLog log = null)
    {
        myPos = gsd.GetValue();
        return true;
    }

    public PointImp()
    {
        myPos = Vector3.zero;
    }

    Vector3 myPos;
};

// SequencePoint<IPoint,    PointImp,       IPointHolderChange>
// SequencePoint<Parent,    Implementation, Changer>
//struct SequencePoint : Parent, PointImp, Status
public class SequencePoint : Status, IPoint, iPointImp
{
    protected Vector3 myPos;
    // IDeletableMember 
    public virtual void deleteMe()
    {
        myChange.onDeletePoint(this);
    }

    // IPoint 
    public virtual Vector3 getPos()
    {
        return myPos;
    }

    public virtual void setPos(Vector3 pos)
    {
        myPos = pos;
        myChange.onChangePoint();
    }

    public bool save(uvv gsd)
    {
        gsd.SetValue(myPos);
        //saveNotify();
        return true;
    }

    public virtual bool load(uvv gsd, ILoadErrorLog log = null)
    {
        myPos = gsd.GetValue();
        return true;
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    // self
    public SequencePoint(IPointHolderChange ch)
    {
        myChange = ch;
    }

    IPointHolderChange myChange;
}