using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ****************************************************************************
// data struct
public class FOOT_DATA : SUBOBJ_DATA
{
    public FOOT_DATA(string name) : base(name)
    {
        SetFlag(SC_FOOT | SF_CRITICAL);
        IsLeft = false;
    }
    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            if (st.LoadBool(ref IsLeft, "IsLeft")) continue;
            base.ProcessToken(st, value);
        } while (false);

    }
    public override void Reference(SUBOBJ_DATA referenceData)
    {
        base.Reference(referenceData);
        // FOOT_DATA
        if (referenceData.GetClass() != SC_FOOT) return;
        FOOT_DATA rr = (FOOT_DATA)referenceData;
        IsLeft = rr.IsLeft;

    }
    public override void MakeLinks()
    {
        base.MakeLinks();
    }
    // data section
    public bool IsLeft;
};
