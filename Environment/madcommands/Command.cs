using UnityEngine;
using crc32 = System.UInt32;
/// <summary>
/// class Command - command
/// </summary>
public class Command : ComBase
{
    int nparam;

    public Command(string nm, crc32 cn, string h, CommLink l, int np) : base(nm, cn, cn, h, l)
    {
        nparam = np;
        ctype = ComType.CM_COMMAND;
    }

    public override string Exec(string myin, bool inv)
    {
        string[] argv = new string[] { null, null };

        for (int i = 0; i < nparam; ++i)
        {
            StrParam param = new StrParam(myin);
            Debug.Log("param " + param + " from [" + myin + "]");
            if (!param.Valid()) return null;

            //argv[i] = ANewN(char, param.ParamLen() + 1);
            //param.CopyTo(argv[i]);
            param.CopyTo(out argv[i]);
            myin = param.End();
        }

        // inversed command does not executes
        if (!inv)
            link.OnCommand(cname, argv[0], argv[1]);
        return myin;
    }
};
