using UnityEngine;
using MarkerBase= GamesetMember<IMarker, IGroupHolderChange>;
using uvv = iUnifiedVariableVector;
//typedef GamesetMember<IMarker, IGroupHolderChange, IGroupHolderChange::onRenameMarker, IGroupHolderChange::onDeleteMarker, IGroupHolderChange::onChangeMarker> MarkerBase;

//public class Marker : MarkerBase, iPointImp,IMarker,ILoadableMember
public class Marker : MarkerBase, IMarker, ILoadableMember
{
    public const uint ID = 0x7A9CDA37;
    // IObject
    public object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IStatus.ID: return getIStatus();
            case Marker.ID: return this;
            default: return 0;
        }
    }

    // IDeletableMember 
    public override void deleteMe()
    {
        myChange.onDeleteMarker(this);
    }
    // IMarker 
    public virtual Vector3 getPos()
    {
        return myPos;
    }

    public virtual void setPos(Vector3 pos)
    {
        myPos = pos;
        myChange.onChangeMarker();
    }

    // self
    public Marker(string name, IGroupHolderChange ch) : base(name, ch) {
        myPos = Vector3.zero;
    }

    //bool iPointImp.save(uvv gsd)
    //{
    //    gsd.SetValue(myPos);
    //    //saveNotify();
    //    return true;
    //}

    private bool load(uvv gsd, ILoadErrorLog log = null)
    {
        myPos = gsd.GetValue();
        return true;
    }
    public override bool load<UniType>(UniType gsd, ILoadErrorLog log) 
    {
        //myPos = ((uvv)gsd).GetValue();
        //return true;
        return load((uvv)gsd, log);
    }

    Vector3 myPos;

    //interface methods
    public void setRadius(float rad)
    {
        Vector3 pos = getPos();
        setPos(new Vector3(pos.x, rad, pos.z));
    }
    public Vector2 getPosition()
    {
        Vector3 pos = getPos();
        return new Vector2(pos.x, pos.z);
    }
    public void setPosition(Vector2 pos )
    {
        setPos(new Vector3(pos.x, getRadius(), pos.y));
    }

    float getRadius() { return getPos().y; }

}
public interface iPointImp
{
    bool save(uvv gsd);

    bool load(uvv gsd, ILoadErrorLog log = null);


};