using crc32 = System.UInt32;
/// <summary>
/// class Trigger - "+/-" command (e.g. +attack)
/// </summary>
public class Trigger : ComBase
{
    bool sign; // true - '+', false '-'

   public Trigger(string nm, crc32 cn, crc32 pn, string h, CommLink l, bool s): base(nm, cn, pn, h, l)
    {
        sign = s;
        ctype = ComType.CM_TRIGGER;
    }

    public override string Exec(string myin , bool inv)
    {
        // вместо sign ^ true было 1-sign
        link.OnTrigger(cname, inv ? sign ^ true: sign);
        return myin;
    }
    public override int Match(string s)
    {
        char sg = sign ? '+' : '-';

        if (!s.StartsWith(sg)) return 0;

        int commch = base.Match(s.Remove(0, 1));
        return commch != 0 ? commch + 256 : 0;
    }
    public override void Suggest(string s, string lim)
    {
        char sg = sign ? '+' : '-';
        if (lim.Length - s.Length > 3)
        {
            //s = sg;
            s = s.Remove(0, 1); //Здесь что-то неправильное.
            base.Suggest(s + 1, lim);
        }
    }
    public override int GetName(out string dest, int size)
    {
        Asserts.Assert(size > 1);
        dest = sign ? "+" : "-";
        dest += name;
        return dest.Length;
        //return __strncpy(dest + 1, name, --size) - dest;
    }
};