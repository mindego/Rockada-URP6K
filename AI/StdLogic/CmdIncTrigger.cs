public class CmdIncTrigger : CmdSetTrigger
{
    // IVmCommand
    public override bool exec()
    {
        int value = 0;
        myTriggers.getTrigger(myName, out value);
        myTriggers.setTrigger(myName, myValue + value);
        return true;
    }

};
