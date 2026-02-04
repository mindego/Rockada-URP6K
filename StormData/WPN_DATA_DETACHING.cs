using System;
/// <summary>
/// оружие с отделяемыми поражающими элементами
/// </summary>
public class WPN_DATA_DETACHING : WPN_DATA
{
    // internal part
    public WPN_DATA_DETACHING(string name) : base(name)
    {
        Mass = 0;
        // список имен отделяемых частей
        nDetachingNames = 0;
        DetachingNames[0] = null;
        DetachingNames[1] = null;
        DetachingNames[2] = null;
        DetachingNames[3] = null;

    }
    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            if (st.LoadFloat(ref Mass, "Mass")) continue;
            // список имен отделяемых частей
            if (st.Recognize("DetachingPart"))
            {
                if (nDetachingNames >= 4) throw new Exception("Too many detacheable parts");
                //DetachingNames[nDetachingNames++] = CodeString(f.GetNextItem());
                DetachingNames[nDetachingNames++] = st.GetNextItem();
                return;
            }
            base.ProcessToken(st, value);

        } while (false);
    }
    public override void Reference(SUBOBJ_DATA data)
    {
        base.Reference(data);
        // поля WPN_DATA
        if (data.GetClass() != SC_WEAPON_SLOT) return;
        WPN_DATA r = (WPN_DATA)data;
        // поля WPN_DATA_DETACHING
        WPN_DATA_DETACHING rr = (WPN_DATA_DETACHING)data;
        Mass = rr.Mass;
        for (nDetachingNames = 0; nDetachingNames < rr.nDetachingNames; nDetachingNames++)
            DetachingNames[nDetachingNames] = rr.DetachingNames[nDetachingNames];

    }

    // список имен отделяемых частей
    public float Mass;
    public int nDetachingNames;
    //DWORD[] DetachingNames = new DWORD[4];
    public string[] DetachingNames = new string[4];
};
