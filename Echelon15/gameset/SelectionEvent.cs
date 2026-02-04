using BaseSelectionEvent = Event<ISelectionEvent, IGamesetChange>;
using uvc = iUnifiedVariableContainer;
//typedef Event<ISelectionEvent, IGamesetChange, IGamesetChange::onRenameSelectionEvent, IGamesetChange::onDeleteSelectionEvent, IGamesetChange::onChange> BaseSelectionEvent;

public class SelectionEvent : BaseSelectionEvent, ISelectionEvent, ILoadableTransMember
{
    public const uint ID = 0x63676558; //Ошибка разработчиков, должно быть 0xEBAB718B
    // IObject
    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IStatus.ID: return getIStatus();
            case SelectionEvent.ID: return this;
            default: return 0;
        }
    }

    // ISelectionEvent
    public virtual string getText(int i)
    {
        return myText[i];
    }

    public virtual void setText(int i, string text)
    {
        if (myText[i] == text) return;
        myText[i] = text;
        myChange.onChange();
        setChanged();
    }

    public virtual string getScript(int i)
    {
        return myScript[i];
    }

    public virtual void setScript(int i, string text)
    {
        if (myScript[i] == text) return;
        myScript[i] = text;
        myChange.onChange();
        setChanged();
    }

    // self
    public override bool save(uvc gsd, uvc gsl)
    {
        if (!base.save(gsd, gsl)) return false;
        gsd.setInt("Type", Hasher.HshString("Selection"));

        if (!gsl.setString("Text", myText[0])) return false;
        for (int i = 1; i != 4; i++)
        {
            string buffer;
            //char buffer[50];
            //wsprintf(buffer, "Selection%dText", i);
            buffer = string.Format("Selection{0}Text", i);
            if (!gsl.setString(buffer, myText[i])) return false;
            //wsprintf(buffer, "Selection%dNextEventScript", i);
            buffer = string.Format("Selection{0}NextEventScript", i);
            if (!gsd.setString(buffer, myScript[i - 1])) return false;
        }
        setNotChanged();
        saveNotify();
        return true;
    }

    public override bool load(uvc gsd, uvc gsl, ILoadErrorLog log = null)
    {
        if (!base.load(gsd, gsl, log)) return false;
        gsl.getString("Text", ref myText[0]);
        for (int i = 1; i != 4; i++)
        {
            string buffer;
            buffer = string.Format("Selection{0}Text", i);
            gsl.getString(buffer, ref myText[i]);
            buffer = string.Format("Selection{0}NextEventScript", i);
            gsd.getString(buffer, ref myScript[i - 1]);
        }

        setNotChanged();
        return true;
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

    public SelectionEvent(string name, IGamesetChange ch) : base(name, ch) { }

    string[] myText = new string[4];
    string[] myScript = new string[3];
}