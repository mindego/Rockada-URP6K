using UnityEngine;
using crc32 = System.UInt32;


public class CmdNotifyAll : BaseCommand
{
    public override int getType()
    {
        return 7; //Забавно. Может где-то есть список? MessageCodes.CMD_NOTIFY ?
    }

    // IVmCommand
    public override bool exec()
    {
        RadioMessage rdm = new RadioMessage();
        rdm.Code = Hasher.HshString(myCode); //TODO хэширование сообщения дублируется в fillOrg. ВОзможно, стоит здесь удалить?
        fillOrg(ref rdm, myCode);
        CallerInfo info = mySender.getInfo();
        // changed
        rdm.CallerCallsign = info.myCallsign;
        rdm.CallerIndex = info.myIndex;
        //myRadio.sendRadioMessage(myCode, myAi, ref rdm, true, false); //TODO 
        myRadio.sendRadioMessage(myCode, myAi, rdm, true, false);
        return true;
    }

    protected void fillOrg(ref RadioMessage rdm, string code)
    {
        rdm.Code = Hasher.HshString(code);
        if (myPool != null)
        {
            IParamList.Param p;
            if (myPool.getVariable(Hasher.HshString("CopyOrg"), out p))
                //rdm.Org = new Vector3(p.myVector[0], p.myVector[1], p.myVector[2]); //TODO Проверить запрлнение координат радиовещателя
                rdm.Org = p.myVector;
            else
                mySender.getOrg(out rdm.Org);
        }
        else
            rdm.Org = Vector3.zero;

    }
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descString(myparams, "Code", myCode);
        return myparams;
    }

    public override bool isParsingCorrect()
    {
        return myCode != null;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Code: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Code: myCode = p.myString; break;
            default: return false;
        }
        return true;
    }

    protected string myCode; protected const crc32 prm_Code = 0x28D86059;

    public override bool setDefaults(IQuery tm)
    {
        myRadio = (IRadioService)tm.Query(IRadioService.ID);
        myAi = mySender.getAi();
        return myRadio != null;
    }
    protected IRadioService myRadio;
    protected IAi myAi;

    public virtual object Query(uint cls_id)
    {
        return null;
    }
}

