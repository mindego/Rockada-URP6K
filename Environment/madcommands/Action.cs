using crc32 = System.UInt32;
/// <summary>
/// class Action - execute string assotiated with name
/// </summary>
public class Action : ComBase
{
    Commands cmd;
    int action_len;
    string action;
    Action pair;

    public Action(string nm, crc32 cn, string act, Commands _cmd) : base(nm, cn, cn)
    {
        cmd = _cmd;
        ctype = ComType.CM_ACTION;
        help = action = null;
        action_len = 0;
        pair = null;
        SetAction(act);
        SetPair();
    }

    void SetNewValue(string act) { } //Похоже не используется

    ~Action()
    {
        if (pair != null)
        {
            Asserts.Assert(pair.pair == this);
            pair.pair = null;
        }
        action = null;
    }

    public override string Exec(string input, bool inv)
    {
        if (inv)
        {
            if (pair != null)
                return pair.Exec(input, false);
            else
            {
                // cmd->log->Message("action error");
            }
        }
        else
        {
            string dup_action = action;
            cmd.ProcessString(dup_action);
        }
        return input;
    }

    /// <summary>
    /// find and set my pair and pair of my pair
    /// </summary>
    void SetPair()
    {
        if (name[0] == '+' || name[0] == '-')
        {
            crc32 c_sign = (name[0] == '+') ? (uint)cmd.mns_code : (uint)cmd.pls_code;

            //ComBase cb = cmd.GetCommand(Crc32.Code(c_sign, name + 1));
            ComBase cb = cmd.GetCommand(Hasher.HshString(name));


            if (cb!=null)
            {
                Asserts.Assert(cb.ctype == ComType.CM_ACTION);
                pair = cb.GetAction();
                Asserts.Assert(pair.pair == null);
                pair.pair = this;
            }
        }
    }
    void SetAction(string act)
    {
        if (action!=null) action = null;
        help = action = act;
        action_len = act.Length;
    }

    public override Action GetAction() { return this; }
};
