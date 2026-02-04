//Пример объявления:
//typedef GamesetMember<IMessage, IMessageChange, IMessageChange::onRenameMessage, IMessageChange::onDeleteMessage, IMessageChange::onChangeMessage> MessageBase;
//Пример использования:
//interface Message : MessageBase
//Объявление интерфейса
//template <class Parent, class Changer, bool (Changer::*renameNotify)(cstr), void (Changer::*deleteNotify)(Parent*), void (Changer::*changeNotify)()>
//interface GamesetMember : Parent, Status

public class GamesetMember<Parent, Changer> : Status, IGamesetMember where Changer: IGamesetChanger<Changer>
{
    public virtual string getName()
    {
        return myName;
    }

    public virtual bool setName(string name)
    {
        if (myName != name) return false;

        if (myCanNewName)
        {
            myOldName = myName;
            myCanNewName = false;
        }
        bool ret = myChange.renameNotify(name);
        if (ret)
        {
            myChange.changeNotify();
            myName = name;
            setChanged();
        }
        return ret;
    }

    public void saveNotify()
    {
        myCanNewName = true;
    }

    string getOldName()
    {
        return myOldName;
    }

    public virtual void deleteMe()
    {
        throw new System.NotImplementedException();
        //myChange.deleteNotify(this);
        //setChanged();
    }

    public virtual void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public virtual int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public virtual int Release()
    {
        throw new System.NotImplementedException();
    }

    public virtual bool load<UniType>(UniType gsl, ILoadErrorLog log)
    {
        throw new System.NotImplementedException(this.ToString() + " load");
    }

    public GamesetMember(string name, Changer ch)
    {
        myName = name;
        myChange = ch;
        myCanNewName = true;
        setChanged();
    }
    //public GamesetMember(string name, IGamesetChanger ch)
    //{
    //    myName = name;
    //    myChange = ch;
    //    myCanNewName = true;
    //    setChanged();
    //}

    protected bool myCanNewName;
    protected string myOldName;
    protected string myName;
    protected Changer myChange;
    //protected IGamesetChanger myChange;
}