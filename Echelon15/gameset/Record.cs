using uvc = iUnifiedVariableContainer;
//using BaseRecord = ScriptableEvent<IRecord, IGamesetChange, IGamesetChange::onRenameRecord, IGamesetChange::onDeleteRecord, IGamesetChange::onChange> BaseRecord;
using BaseRecord = ScriptableEvent<IRecord, IGamesetChange>;

public class Record : BaseRecord,IRecord,ILoadableTransMember
{
    public const uint ID = 0x63676558;
    // IObject
    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IStatus.ID: return getIStatus();
            case Record.ID: return this;
            default: return 0;
        }
    }

    // IRecord
    public virtual string getText()
    {
        return myText;
    }

    public virtual void setText(string text)
    {
        if (myText == text) return;
        myText = text;
        myChange.onChange();
        setChanged();
    }

    public virtual string getVideo()
    {
        return myVideo;
    }

    public virtual void setVideo(string text)
    {
        if (myVideo == text) return;
        myVideo = text;
        myChange.onChange();
        setChanged();
    }

    public virtual string getAudio()
    {
        return myAudio;
    }

    public virtual void setAudio(string text)
    {
        if (myAudio == text) return;
        myAudio = text;
        myChange.onChange();
        setChanged();
    }

    // self
    public override bool save(uvc gsd, uvc gsl)
    {
        if (!base.save(gsd, gsl)) return false;
        if (!gsl.setString("Text", myText)) return false;
        //gsd.setInt("Type", 0x63676558);
        gsd.setInt("Type", Record.ID);
        if (!gsd.setString("AudioName", myAudio)) return false;
        if (!gsd.setString("VideoName", myVideo)) return false;
        setNotChanged();
        saveNotify();
        return true;
    }

    public override bool load(uvc gsd, uvc gsl, ILoadErrorLog log = null)
    {
        if (!base.load(gsd, gsl, log)) return false;
        gsd.getString("AudioName", ref myAudio);
        gsd.getString("VideoName", ref myVideo);
        if (!Savings.loadStringVar("Text", ref myText, gsl, log)) return false;
        setNotChanged();
        return true;
    }

    public Record(string name, IGamesetChange ch) : base(name, ch) { }

    protected string myText;
    protected string myVideo;
    protected string myAudio;
};
