public interface IRadioHandler
{
    public bool checkMessage(RadioMessage m);
};

public interface IRadioService
{
    public const uint ID = 0x19C2F287;
    public bool isRadioFree();
    public void sendRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all, bool say_flag);

    public void notifyRadioMessage(RadioMessage Info);

    public void registerHandler(IRadioHandler rh);
    public void unregisterHandler(IRadioHandler rh);
};

