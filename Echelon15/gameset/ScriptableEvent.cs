using uvc = iUnifiedVariableContainer;
//template <class Parent, class Changer, bool(Changer::* renameNotify)(string), void(Changer::* deleteNotify)(Parent *), void(Changer::* changeNotify)() >
//public class ScriptableEvent : Event<Parent, Changer, renameNotify, deleteNotify, changeNotify>
public class ScriptableEvent<Parent, Changer> : Event<Parent, Changer> where Parent : IGamesetMember where Changer : IGamesetChanger<Changer>
{
    public virtual string getNextScript()
    {
        return myNextScript;
    }

    public virtual void setNextScript(string script)
    {
        if (script != myNextScript)
        {
            myNextScript = script;
            myChange.changeNotify();
            setChanged();
        }
    }
    public override bool load(uvc gsd, uvc gsl, ILoadErrorLog log = null)
    {
        if (!base.load(gsd, gsl, log)) return false;
        gsd.getString("NextEventScript", ref myNextScript);
        return true;
    }
    public override bool save(uvc gsd, uvc gsl)
    {
        if (!base.save(gsd, gsl)) return false;
        bool ret = gsd.setString("NextEventScript", myNextScript);
        if (ret) saveNotify();
        return ret;
    }

    public ScriptableEvent(string name, Changer ch) : base(name, ch) { }
    protected string myNextScript;
};