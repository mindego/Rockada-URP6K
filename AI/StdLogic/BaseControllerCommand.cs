using crc32 = System.UInt32;

public abstract class BaseControllerCommand : BaseCommand
{
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        return myParams!=null ? myParams.getParamInfo(param_name) : new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        return myParams !=null ? myParams.addParameter(param_name, op, p, real_name) : false;
    }

    public override bool isParsingCorrect()
    {
        if (myController!=null)
            myController.setID(getID());
        return myController !=null ;
    }

    public override bool restart()
    {
        if (myController!=null)
            myController.restart();
        return myController!=null;
    }


    // BaseControllerCommand
    public abstract crc32 getID();

    public void setController(IVmController cont)
    {
        myController = cont;
        if (cont!=null)
        {
            //cont->AddRef();
            myParams = cont.getParamList();
        }
        else
            myParams = null;
        myIsSequence = myParams!=null;
    }
    IParamList myParams;
    protected IVmController myController;
}