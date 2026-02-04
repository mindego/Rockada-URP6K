public class CmdSetGlobalAccessToWeapon : BaseGlobalAccessCommand
{
    // IVmCommand
    public override bool exec()
    {
        myData.setAccess(false, myName, ICoopMission.getAccessFromCrc(Hasher.HshString(myAccess)));
        return true;
    }
};
