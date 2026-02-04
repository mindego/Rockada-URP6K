using uvc = iUnifiedVariableContainer;
//template <class Parent, class Changer, bool(Changer::* renameNotify)(string), void(Changer::* deleteNotify)(Parent *), void(Changer::* changeNotify)() >
//class Event : GamesetMember<Parent, Changer, renameNotify, deleteNotify, changeNotify> where Changer: IGamesetChange
//Вызывается как 
//typedef Event<ISelectionEvent, IGamesetChange, IGamesetChange::onRenameSelectionEvent, IGamesetChange::onDeleteSelectionEvent, IGamesetChange::onChange> BaseSelectionEvent;
//interface ScriptableEvent : Event<Parent, Changer, renameNotify, deleteNotify, changeNotify>


    public class Event<Parent, Changer> : GamesetMember<Parent,Changer> where Parent: IGamesetMember where Changer : IGamesetChanger<Changer>
{
    public virtual bool isChanged()
    {
        return (getStatus() != ChangeStatus.csNotChanged);
    }

    public virtual string getDescription()
    {
        return myDesc;
    }
    public virtual void setDescription(string desc)
    {
        if (myDesc == desc) return;
        myDesc = desc;
        myChange.changeNotify();
        setChanged();
    }

    public virtual string getTitle()
    {
        return myTitle;
    }

    public virtual void setTitle(string desc)
    {
        if (myTitle == desc) return;
        myTitle = desc;
        myChange.changeNotify();
        setChanged();
    }

    public virtual TimeInfo getTime()
    {
        return myTime;
    }

    public virtual void setTime(TimeInfo ti)
    {
        if (!(ti == myTime))
        {
            myTime = ti;
            setChanged();
            myChange.changeNotify();
        }
    }

    public virtual bool save(uvc gsd, uvc gsl)
    {
        gsl.setString("Title", myTitle);
        gsl.setString("Description", myDesc);
        gsd.setInt("DateYear", myTime.wYear);
        gsd.setInt("DateMonth", myTime.wMonth);
        gsd.setInt("DateDay", myTime.wDay);
        gsd.setInt("DateMinute", myTime.wMinute);
        gsd.setInt("DateHour", myTime.wHour);
        gsd.setInt("DateIsOffset", myTime.IsOffset);

        saveNotify();
        return true;
    }

    public virtual bool load(uvc gsd, uvc gsl, ILoadErrorLog log = null)
    {
        gsd.getInt("DateYear", out myTime.wYear);
        gsd.getInt("DateMonth", out myTime.wMonth);
        gsd.getInt("DateDay", out myTime.wDay);
        gsd.getInt("DateMinute", out myTime.wMinute);
        gsd.getInt("DateHour", out myTime.wHour);
        gsd.getInt("DateIsOffset", out myTime.IsOffset);
        if (!Savings.loadStringVar("Description", ref myDesc, gsl, log)) return false;
        return Savings.loadStringVar("Title", ref myTitle, gsl, log);
    }

    public Event(string name, Changer ch) : base(name, ch) { }
    protected string myDesc;
    protected string myTitle;
    protected TimeInfo myTime = new TimeInfo();
};