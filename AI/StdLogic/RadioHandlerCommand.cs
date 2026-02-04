using UnityEngine;

public abstract class RadioHandlerCommand : BaseControllerCommand, IRadioHandler
{
    ~RadioHandlerCommand()
    {
        registerMe(false);
    }

    // IVmCommand
    public override bool exec()
    {
        bool ret = false;
        if (myStarted)
        {
            if (myRetOnNextTick)
            {
                ret = true;
                setStarted(false);
            }
            myRetOnNextTick = !myRetOnNextTick;
        }
        /*        if (ret)
                    printMessage("starting",0);*/
        return ret;
    }

    // BaseCommand
    public override bool restart()
    {
        registerMe(true);
        return base.restart();
    }

    public override void onOverride()
    {
        registerMe(false);
    }

    public override  bool setDefaults(IQuery tm)
    {
        myRadio = (IRadioService) tm.Query(IRadioService.ID);
        return myRadio!=null;
    }

    IRadioService myRadio;
    bool myRetOnNextTick = false;

    protected bool setStarted(bool started)
    {
        //printMessage(started?"setStarted":"setNotStarted",0);
        //Debug.Log(string.Format("Started handler {0} {1}", this.myCommandName,this));
        myStarted = started; return started;
    }
    private void registerMe(bool reg)
    {
        //printMessage(reg?"Registering":"Unregistering",0);
        if (myRadio == null) return;

        if (reg) myRadio.registerHandler(this); else myRadio.unregisterHandler(this);
    }

    public abstract bool checkMessage(RadioMessage m);

    bool myStarted = false;
}