using System.Collections.Generic;
using crc32 = System.UInt32;

public class CmdOnDamage : BaseControllerCommand
{
    // IVmCommand
    public override bool exec()
    {
        myInfo.refresh(ref myParts);
        for (int i = 0; i < myParts.Count; ++i)
        {
            if (myParts[i].myCondition < myRange)
            {
                myInfo.stopRefresh();
                return true;
            }
        }
        return false;
    }
    // BaseCommand
    public override string  describeParams(ref string myparams)
    {
        myparams = descStrings (myparams,"Name",myName);
        myparams = descFloat (myparams,"Range",myRange);
        return myparams;
    }

    public override bool restart()
    {
        myParts.Clear();
        List<string> root=new List<string>();
        if (myName.Count!=0)
            myInfo.fill(myName.ToArray(), ref myParts);
        else
        {
            root.Add("");
            myInfo.fill(root.ToArray(), ref myParts);
        }
        return base.restart();
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        string cur = "";
        for (int i = 0; i < myName.Count; ++i)
            cur += string.Format("{0}",myName[i]);
        cur += string.Format("{0}", (int) myRange * 100);
        return Hasher.Code(0x164F3796, cur);
    }


    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Name: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, IParamList.OpType.OP_EQU);
            case prm_Range: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, IParamList.OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            //case prm_Name: myName.Add(p.myString); break;
            case prm_Name: myName.Add(p.myString.Replace("\"",null)); break; //TODO Удаляем кавычки. Правильнее удалять их на этапе загрузки
            case prm_Range: myRange = p.myFloat;break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    List<string> myName=new List<string>();  const crc32 prm_Name = 0x01EE2EC7;
    float myRange;  const crc32 prm_Range = 0xADB98AB2;

    public override bool setDefaults(IQuery tm)
    {
        myInfo = (IUnitService) tm.Query(IUnitService.ID);
        myRange = 0f;
        return myInfo!=null;
    }

    IUnitService myInfo;
    List<PartInfo> myParts = new List<PartInfo>();
};