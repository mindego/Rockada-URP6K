public class CmdSetGlobalAccessToCraft : BaseGlobalAccessCommand
{
    // IVmCommand
    public override bool exec()
    {
        myData.setAccess(true, myName, ICoopMission.getAccessFromCrc(Hasher.HshString(myAccess)));
        return true;
    }
};
