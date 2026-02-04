using uvc = iUnifiedVariableContainer;
using uvv = iUnifiedVariableVector;
using tuva = iUnifiedVariableArray;
using System.Collections.Generic;
//template <class Changer, class SubItemImpl, class SubItem, class SubItemChanger, class ParentClass>
//interface Sequence : GamesetMember<ParentClass, Changer, Changer::onRenameRoad, Changer::onDeleteRoad, Changer::onChangeRoad>, SubItemChanger

//Вызывается как:
//interface RoadD : Sequence<IRoadHolderChange, Point, IPoint, IPointHolderChange, IRoad>
//interface RouteD : Sequence<IRouteHolderChange, RoutePoint, IRoutePoint, IRoutePointHolderChange, IRoute> { - Кодом не используется
//<Changer,             SubItemImpl,    SubItem,    SubItemChanger,     ParentClass>
//<IRoadHolderChange,   Point,          IPoint,     IPointHolderChange, IRoad>
public class Sequence : GamesetMember<IRoad, IRoadHolderChange> , IPointHolderChange
{

    // IDeletableMember 
    public override void deleteMe()
    {
        setDeleted();
        //myChange.onDeleteRoad(this);
    }

    // IRoad
    public virtual IPoint getPoint(int i)
    {
        if (i >= myPoints.Count) return null;
        return myPoints[i];
        //SubItem rec = myPoints.get(i);
        //return IRefMem.SafeAddRef(rec);
    }

    public virtual IPoint createPoint()
    {
        myChange.onChangeRoad();
        setChanged();
        return createPointLcl();
    }

    Point createPointLcl()
    {
        var rec = new Point(this);
        myPoints.Add(rec);
        return rec;
    }

    public virtual IPoint insertPoint(int pos)
    {
        myChange.onChangeRoad();
        setChanged();
        var rec = new Point(this);
        myPoints.Add(rec);
        return rec;
        //return myPoints.insert(this, pos);
    }

    public virtual void onChangePoint()
    {
        myChange.onChangeRoad();
        setChanged();
    }

    public Sequence(IRoadHolderChange ch, IStormData rs) : base("", ch)
    {
        myChange = ch;
        myData = rs;
        setChanged();
        Asserts.Assert(myData!=null);
    }

    public virtual bool save(uvc gsd)
    {
        gsd.setInt("Name", Hasher.HshString(myName));
        tuva ar;
        if ((ar = gsd.createArray("Points"))!=null)
            Savings.saveSimpleArray<Point, uvv, IPointHolderChange>(ar, myPoints);
        setNotChanged();
        return true;
    }

    public virtual bool load(uvc gsd, ILoadErrorLog log = null)
    {
        uint data = Constants.THANDLE_INVALID;
        if (!Savings.loadIntVar("Name", ref data, gsd, log)) return false;

        string name;
        if ((name = myData.resolveRoadName(data))!=null)
            myName = name;
        else
        {
            myData.processIntegrityFailure();
            Savings.addResolveWarning(gsd, log);
        }

        tuva ar = gsd.createArray("Points");
        Savings.enumSimpleArray<Point, uvv, Sequence>(ar, this, createPointLcl, log);
        setNotChanged();
        return true;
    }

    public virtual void onDeletePoint(IPoint p)
    {
        throw new System.NotImplementedException();
    }

    new IRoadHolderChange myChange;
    IStormData myData;

    //CommonMap<SubItemImpl, SubItemChanger> myPoints;
    protected List<Point> myPoints = new();
}