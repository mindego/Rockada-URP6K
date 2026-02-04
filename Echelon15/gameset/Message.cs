using MessageBase = GamesetMember<IMessage, IMessageChange>;
using uvs = iUnifiedVariableString;
//typedef GamesetMember<IMessage, IMessageChange, IMessageChange::onRenameMessage, IMessageChange::onDeleteMessage, IMessageChange::onChangeMessage> MessageBase;

public class Message : MessageBase, IMessage,ILoadableMember
{
    public const uint ID = 0x86FFF61C;
    // IObject
    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IStatus.ID: return getIStatus();
            case Message.ID: return this;
            default: return 0;
        }
    }

    public virtual string getText()
    {
        return myText;
    }

    public virtual void setText(string name)
    {
        if (name == myText) return;
        myText = name;
        myChange.onChangeMessage();
    }

    // self
    public Message(string name, IMessageChange ch) : base(name, ch) { }

    public virtual bool save(uvs cont)
    {
        bool ret = cont.SetValue(myText);
        if (ret) saveNotify();
        return ret;
    }

    public virtual bool load(uvs cont, ILoadErrorLog log = null)
    {
        //char* buffer = ANewN(char, cont->StrLen() + 1);
        //cont->StrCpy(buffer);
        //myText = buffer;
        myText = cont.GetValue();
        return true;
    }

    public override bool load<UniType>(UniType gsl, ILoadErrorLog log)
    {
        return this.load((uvs)gsl, log);
    }

    string myText;
}