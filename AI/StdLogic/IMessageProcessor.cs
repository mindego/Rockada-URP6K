using crc32 = System.UInt32;

public interface IMessageProcessor : IObject
{
    public bool presentMessage(IGroupAi ai, crc32 callsign_name, crc32 msg, bool sender, int index = -1);
    public void AddMessage(RadioMessage Info, RadioMessageInfo RMparams, IGroupAi grp);
    public void Update(float scale);

    public void onGroupDestroy(IGroupAi ai);
};
