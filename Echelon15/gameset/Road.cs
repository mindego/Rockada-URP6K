using uvc = iUnifiedVariableContainer;
public class RoadD : Sequence, IRoad,ILoadableMember
{
    public const uint ID = 0x217EF921;
    // IObject
    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IStatus.ID: return getIStatus();
            case RoadD.ID: return this;
            default: return null;
        }
    }
    public override void onDeletePoint(IPoint pnt)
    {
        myPoints.Remove((Point)pnt.Query(Point.ID));
        myChange.onChangeRoad();
        setChanged();
    }
    public RoadD(IRoadHolderChange ch, IStormData rs) : base(ch, rs) { }
    // wrappers for dummy boost
    public override IPoint createPoint()
    {
        return base.createPoint();
    }
    public override IPoint getPoint(int i)
    {
        return base.getPoint(i);
    }
    public override IPoint insertPoint(int pos)
    {
        return base.insertPoint(pos);
    }
    public override bool load(uvc gsd, ILoadErrorLog log = null)
    {
        return base.load(gsd, log);
    }

    public override bool load<UniType>(UniType gsd, ILoadErrorLog log = null)
    {
        return load((uvc)gsd, log);
    }
    public override bool save(uvc gsd)
    {
        bool ret = base.save(gsd);
        if (ret) saveNotify();
        return ret;
    }
};
