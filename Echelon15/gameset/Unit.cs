using UnityEngine;
using UnitBase = GamesetMember<IUnit, IGroupChange>;
using uvc = iUnifiedVariableContainer;
//typedef GamesetMember<IUnit, IGroupChange, IGroupChange::onRenameUnit, IGroupChange::onDeleteUnit, IGroupChange::onChangeUnit> UnitBase;


public class Unit : UnitBase, IUnit,ILoadableMember
{
    public const uint ID = 0x83765C92;
    // IObject
    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IStatus.ID: return getIStatus();
            case Unit.ID: return this;
            default: return 0;
        }
    }

    public override bool setName(string name)
    {
        base.setName(name);
        if (myData!=null)
        {
            UnitAttribute p = myData.resolveUnitAttr(Hasher.HshString(name));
            myAttr = p.myType;
        }
        myChange.onChangeUnit();
        return true;
    }


    public virtual string getAi() { return myAi; }
    public virtual void setAi(string text) { myAi = text; myChange.onChangeUnit(); }

    public virtual string getAiScript() { return myAiScript; }
    public virtual void setAiScript(string text) { myAiScript = text; myChange.onChangeUnit(); }

    public virtual float getAngle() { return myAngle; }
    public virtual void setAngle(float angle) { myAngle = angle; myChange.onChangeUnit(); }

    public virtual int getFlags() { return myFlags; }
    public virtual void setFlags(int flags) { myFlags = flags; myChange.onChangeUnit(); }

    public virtual uint getLayout(int i) { return (i >= 0 && i < 4) ? myLayouts[i] : 0; }
    public virtual void setLayout(int i, uint name)
    {
        if (i >= 0 && i < 4)
            myLayouts[i] = name;
        myChange.onChangeUnit();
    }

    public virtual Vector3 getPos() { return myPos; }
    public virtual void setPos(Vector3 pos) { myPos = pos; myChange.onChangeUnit(); }

    // self
    bool save(uvc gsd)
    {
        /* ????????????????? */
        //myFlags |= CF_APPEAR_NEW_FORMAT; 
        if (!gsd.setString("Ai", myAi)) return false;
        if (!gsd.setString("AiScript", myAiScript)) return false;
        gsd.setFloat("Angle", myAngle);
        if (!gsd.setVector("Org", myPos)) return false;
        gsd.setInt("Flags", myFlags);
        gsd.setInt("Name", Hasher.HshString(myName));
        //char buffer[MAX_PATH];
        string buffer;
        for (int i = 0; i < 4; ++i)
        {
            //wsprintf(buffer, "Layout%d", i + 1);
            buffer = string.Format("Layout{0}", i + 1);
            gsd.setInt(buffer, myLayouts[i]);

        }
        return true;
    }

    bool load(uvc gsd, ILoadErrorLog log = null)
    {
        if (!Savings.loadStringVar("Ai",ref myAi, gsd, log)) return false;
        if (!Savings.loadStringVar("AiScript", ref myAiScript, gsd, log)) return false;
        if (!gsd.getFloat("Angle", ref myAngle)) return false;
        if (!gsd.getVector("Org", out myPos)) return false;
        if (!Savings.loadIntVar("Flags", ref myFlags, gsd, log)) return false;
        uint data=0;
        if (!Savings.loadIntVar("Name", ref data, gsd, log)) return false;
        //Debug.Log("Loading Attributes");
        UnitAttribute p = myData.resolveUnitAttr(data);
        myAttr = p.myType;
        string name;
        name = myData.resolveObjectName(data);
        //Debug.Log("Resolving object name: " + name);
        if (name != null)
        {
            myName = name;
        }
        else
        {
            Debug.Log("error Loading objname start" );
            myData.processIntegrityFailure();
            Savings.addResolveWarning(gsd, log);
            Debug.Log("error Loading objname end");
            return false;
        }
        //char buffer[MAX_PATH];
        //Debug.Log("Loading Layouts");
        string buffer;
        for (int i = 0; i < 4; ++i)
        {
            //wsprintf(buffer, "Layout%d", i + 1);
            //if (!gsd.getInt(buffer, data)) return false;
            //myLayouts[i] = data;
            buffer = string.Format("Layout{0}", i + 1);
            if (!gsd.getInt(buffer, out data)) return false;
        }

        return true;
    }

    public override bool load<UniType>(UniType gsd, ILoadErrorLog log)
    {
        return load((uvc)gsd, log);
    }

    public virtual UnitType getAttr()
    {
        return myAttr;
    }

    public override void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public override int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public override int Release()
    {
        throw new System.NotImplementedException();
    }

    //public Unit(IGroupChange ch, IStormData rs) : base("", ch)
    public Unit(IGroupChange ch, IStormData rs) : base(null, ch)
    {
        myAngle = 0f;
        myFlags = 0;
        myPos = Vector3.zero;
        myData = rs;
        myAttr = UnitType.utLast;
        for (int i = 0; i < 4; ++i)
            myLayouts[i] = CRC32.CRC_NULL;
        Asserts.Assert(myData!=null);
    }


    string myAi;
    string myAiScript;
    float myAngle;
    int myFlags;
    Vector3 myPos;
    uint[] myLayouts = new uint[4];
    UnitType myAttr;


    IStormData myData;
}