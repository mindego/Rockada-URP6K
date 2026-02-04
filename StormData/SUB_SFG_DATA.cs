using crc32 = System.UInt32;

public class SUB_SFG_DATA : SUBOBJ_DATA
{
    public SUB_SFG_DATA(string name) : base(name)
    {
        SetFlag(SC_SFG);
        SetFlag(SF_DETACHED);
        myRadius = 0f;
        myName = 0xFFFFFFFF;
    }

    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            if (st.LdHS(ref myName, "SfName")) continue;
            if (st.LoadFloat(ref myRadius, "SfRadius")) continue;
            base.ProcessToken(st, value);
        } while (false);

    }
    public override void Reference(SUBOBJ_DATA referenceData)
    {
        base.Reference(referenceData);
        if (referenceData.GetClass() != SC_SFG) return;
        // HANGAR_DATA
        SUB_SFG_DATA rr = (SUB_SFG_DATA)referenceData;
        myName = rr.myName;
        myRadius = rr.myRadius;
        if (myName == 0xFFFFFFFF || myRadius <= 0)
            stormdata_dll.StructError("SFG", FullName);

    }


    public crc32 myName;
    public float myRadius;
};
