using uvc = iUnifiedVariableContainer;
using UnityEngine;

// SequencePoint<IRoutePoint,PointImp, IGroupChange>
// SequencePoint<Parent,    Implementation, Changer>
//struct SequencePoint : Parent, PointImp, Status
public class RoutePoint : Status, IRoutePoint, iPointImp,ILoadableMember
{
    public const uint ID = 0x36A65191;
    // IObject
    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IStatus.ID: return getIStatus();
            case RoutePoint.ID: return this;
            default: return null;
        }
    }

    public virtual void setAiScript(string script)
    {
        myAiScript = script;
    }

    public virtual string getAiScript()
    {
        return myAiScript;
    }

    public RoutePoint(IGroupChange ch) : base() {
        myChange = ch;
    }
    // wrappers for dummy boost
    public Vector3 getPos()
    {
        return myPos;
    }
    public void setPos(Vector3 pos)
    {
        //base.setPos(pos);
        myPos = pos;
    }
    public bool save(uvc gsd)
    {
        if (!gsd.setString("AiScript", myAiScript)) return false;
        if (!gsd.setVector("Org", myPos)) return false;
        // old compatibility
        gsd.setFloat("TimeToPoint", 0);
        //saveNotify();
        return true;
    }

    public bool load(uvc gsd, ILoadErrorLog log = null)
    {
        if (!Savings.loadStringVar("AiScript", ref myAiScript, gsd, log)) return false;
        if (!gsd.getVector("Org", out myPos)) return false;
        return true;
    }

    public bool save(iUnifiedVariableVector gsd)
    {
        throw new System.NotImplementedException();
    }

    public bool load(iUnifiedVariableVector gsd, ILoadErrorLog log = null)
    {
        throw new System.NotImplementedException();
    }

    public void deleteMe()
    {
        throw new System.NotImplementedException();
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

    public bool load<UniType>(UniType type, ILoadErrorLog log)
    {
        return load((uvc)type, log);
    }

    IGroupChange myChange;
    Vector3 myPos;
    string myAiScript;
}

//class RoutePoint : SequencePoint<IRoutePoint, iPointImp, IGroupChange>, IRoutePoint
//{
//    public const uint ID = 0x36A65191;
//    // IObject
//    public virtual object Query(uint cls_id)
//    {
//        switch (cls_id)
//        {
//            case IStatus.ID: return getIStatus();
//            case RoutePoint.ID: return this;
//            default: return null;
//        }
//    }

//    public virtual void setAiScript(string script)
//    {
//        myAiScript = script;
//    }

//    public virtual string getAiScript()
//    {
//        return myAiScript;
//    }

//    public RoutePoint(IGroupChange ch) : base(ch) { }
//    // wrappers for dummy boost
//    public override Vector3 getPos()
//    {
//        return base.getPos();
//    }
//    public override void setPos(Vector3 pos)
//    {
//        base.setPos(pos);
//    }
//    bool save(uvc gsd)
//    {
//        if (!gsd.setString("AiScript", myAiScript)) return false;
//        if (!gsd.setVector("Org", myPos)) return false;
//        // old compatibility
//        gsd.setFloat("TimeToPoint", 0);
//        //saveNotify();
//        return true;
//    }

//    bool load(uvc gsd, ILoadErrorLog log = null)
//    {
//        if (!Savings.loadStringVar("AiScript", ref myAiScript, gsd, log)) return false;
//        if (!gsd.getVector("Org", myPos)) return false;
//        return true;
//    }

//    string myAiScript;
//};