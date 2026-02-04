using crc32 = System.UInt32;

public interface IGameHolder : IObject
{
    public void open(IGameset info, ILocationHolder hld, string profile_name, bool host);
    public bool start(object a, Flags flags);

    public bool update(float Scale, bool UseControls);
    public void draw(float[] pViewport);

    public void close();

    public void applyCraftSelection(ref Selection sel);

    public uint getId();
    public bool isStarted();
    public bool isRestartSupported();

    public void pause(bool on);

    //public ILog getChatLog();
    public crc32 getMissionId();
};


