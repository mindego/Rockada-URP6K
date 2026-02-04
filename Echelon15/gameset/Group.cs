using GroupBase = GamesetMember<IGroup, IGroupHolderChange>;
using uvc = iUnifiedVariableContainer;
using tuva = iUnifiedVariableArray;
using System.Collections.Generic;
using UnityEngine;
//typedef GamesetMember<IGroup, IGroupHolderChange, IGroupHolderChange::onRenameGroup, IGroupHolderChange::onDeleteGroup, IGroupHolderChange::onChangeGroup> GroupBase;

public class Group : GroupBase, IGroupChange, IGroup,ILoadableMember
{
    public const uint ID = 0x53FE943E;
    // IObject
    public object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IStatus.ID: return getIStatus();
            case Group.ID: return this;
            default: return null;
        }
    }
    // IDeletableMember 
    public override void deleteMe()
    {
        myChange.onDeleteGroup(this);
        setDeleted();
    }
    // IGroup
    public virtual string getAi() { return myAi; }
    public virtual void setAi(string text) { myAi = text; setChanged(); myChange.onChangeGroup(); }

    public virtual string getAiScript() { return myAiScript; }
    public virtual void setAiScript(string text) { myAiScript = text; setChanged(); myChange.onChangeGroup(); }

    public virtual int getSide() { return mySide; }
    public virtual void setSide(int side) { mySide = side; setChanged(); myChange.onChangeGroup(); }

    public virtual int getVoice() { return myVoice; }
    public virtual void setVoice(int voice) { myVoice = voice; setChanged(); myChange.onChangeGroup(); }

    public virtual int getFlags() { return myFlags; }
    public virtual void setFlags(int flags) { myFlags = flags; setChanged(); myChange.onChangeGroup(); }

    public virtual IUnit getUnit(int i)
    {
        if (i >= myUnits.Count) return null;
        return myUnits[i];

        //IUnit rec = myUnits.get(i);
        //return IRefMem.SafeAddRef(rec);
    }

    public virtual IUnit createUnit()
    {
        setChanged();
        myChange.onChangeGroup();
        return createUnitLocal();
    }

    public virtual IUnit insertUnit(int pos)
    {
        setChanged();
        myChange.onChangeGroup();
        return insertUnitLocal(pos);
    }

    // IGroupChange {
    public virtual void onDeleteUnit(IUnit un)
    {
        setChanged();
        myChange.onChangeGroup();
        myUnits.Remove((Unit)un.Query(Unit.ID));
    }

    // IGroupChange {
    public virtual void onChangeUnit()
    {
        setChanged();
        myChange.onChangeGroup();
    }

    // IRouteChange
    public virtual void onChangePoint()
    {
        setChanged();
        myChange.onChangeGroup();
    }
    public virtual void onDeletePoint(IRoutePoint un)
    {
        setChanged();
        myRoutePoints.Remove((RoutePoint)un.Query(RoutePoint.ID));
        myChange.onChangeGroup();
    }

    // self
    Unit createUnitLocal()
    {
        //return myUnits.add(this, myData);
        Unit unit = new Unit(this, myData);
        myUnits.Add(unit);
        return unit;
    }

    // self
    Unit insertUnitLocal(int pos)
    {
        Unit unit = new Unit(this, myData);
        myUnits.Insert(pos, unit);
        return unit;
    }

    bool save(uvc gsd)
    {
        setNotChanged();
        if (!gsd.setString("Ai", myAi)) return false;
        if (!gsd.setString("AiScript", myAiScript)) return false;
        gsd.setInt("Flags", myFlags);
        gsd.setInt("Voice", myVoice);
        gsd.setInt("Side", mySide);
        tuva aru = gsd.createArray("Units");
        if (aru != null)
            Savings.saveSimpleArray<Unit, uvc, IGroupChange>(aru, myUnits);
        tuva arp = gsd.createArray("Points");
        if (arp!=null)
            Savings.saveSimpleArray<RoutePoint, uvc, IGroupChange>(arp, myRoutePoints);
        saveNotify();
        return true;
    }

    bool load(uvc gsd, ILoadErrorLog log = null)
    {
        setNotChanged();
        if (!Savings.loadStringVar("Ai", ref myAi, gsd, log)) return false;
        if (!Savings.loadStringVar("AiScript",ref myAiScript, gsd, log)) return false;
        if (!Savings.loadIntVar("Flags", ref myFlags, gsd, log)) return false;
        if (!Savings.loadIntVar("Voice", ref myVoice, gsd, log)) return false;
        if (!Savings.loadIntVar("Side", ref mySide, gsd, log)) return false;
        //Debug.Log("Loading points");
        tuva arp = gsd.createArray("Points");
        if (arp!=null)
            Savings.enumSimpleArray<RoutePoint, uvc, Group>(arp, this, createRoutePointLocal, log);
        //Debug.Log("Loading units");
        tuva aru = gsd.createArray("Units");
        if (aru!=null)
            Savings.enumSimpleArray<Unit, uvc, Group>(aru, this, createUnitLocal, log);
        //Debug.Log("Loading group " + this.myName + "done");
        return true;
    }
    public override bool load<UniType>(UniType gsd, ILoadErrorLog log)
    {
        return load((uvc)gsd, log);
    }

    public Group(string name, IGroupHolderChange ch, IStormData rs) : base(name, ch)
    {
        mySide = 0;
        myVoice = 0;
        myFlags = 0;
        myData = rs;
    }
    public virtual IRoutePoint createPoint()
    {
        IRoutePoint rd = createRoutePointLocal();
        if (rd!=null)
            myChange.onChangeGroup();
        return rd;
    }

    public virtual IRoutePoint insertPoint(int pos)
    {
        myChange.onChangeGroup();
        RoutePoint rp = new RoutePoint(this);
        myRoutePoints.Insert(pos, rp);
        return rp;
    }

    public virtual IRoutePoint getRoutePoint(int i)
    {
        if (i >= myRoutePoints.Count) return null;
        IRoutePoint rec = myRoutePoints[i];
        return IRefMem.SafeAddRef(rec);
    }

    private RoutePoint createRoutePointLocal()
    {
        RoutePoint rp = new RoutePoint(this);
        myRoutePoints.Add(rp);
        return rp;
    }

    public bool renameNotify(string s)
    {
        throw new System.NotImplementedException();
    }

    public void changeNotify()
    {
        throw new System.NotImplementedException();
    }

    public void deleteNotify(IGamesetMember gsm)
    {
        throw new System.NotImplementedException();
    }

    public string myAi;
    public string  myAiScript;
    public int mySide;
    public int myVoice;
    public int myFlags;

    IStormData myData;

    //CommonMap<Unit, IGroupChange> myUnits;
    //CommonMap<RoutePoint, IGroupChange> myRoutePoints;
    List<Unit> myUnits = new List<Unit>();
    List<RoutePoint> myRoutePoints = new List<RoutePoint>();
}
