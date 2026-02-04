using UnityEngine;
/// <summary>
/// data struct
/// </summary>
public class CRAFT_PART_DATA : SUBOBJ_DATA
{
    public CRAFT_PART_DATA(string n) : base(n)
    {
        SetFlag(SUBOBJ_DATA.SC_CRAFT_PART);
        RotateC.Set(0, 0, 0);
        MinusC.Set(0, 0, 0);
        PlusC.Set(0, 0, 0);
        AeroC.Set(0, 0, 0);
        DamageXY[2] = 0;

    }
    public override void ProcessToken(READ_TEXT_STREAM st,string value)
    {
        do
        {
            if (st.Recognize("DamageXY")) {
                DamageXY[0] = st.AtoF(st.GetNextItem());
                DamageXY[1] = st.AtoF(st.GetNextItem());
                DamageXY[2] = st.AtoF(st.GetNextItem());
                DamageXY[3] = st.AtoF(st.GetNextItem());
                continue;
            }
            if (st.Recognize("DamageUV")) {
                DamageUV[0] = st.AtoF(st.GetNextItem());
                DamageUV[1] = st.AtoF(st.GetNextItem());
                DamageUV[2] = st.AtoF(st.GetNextItem()) + DamageUV[0];
                DamageUV[3] = st.AtoF(st.GetNextItem()) + DamageUV[1];
                continue;
            }
            if (st.LoadVector(ref RotateC, "Position")) continue;
            if (st.LoadVector(ref AeroC, "AeroC")) continue;
            if (st.LoadFloatPair(ref MinusC.x, ref PlusC.x, "ThrustX")) continue;
            if (st.LoadFloatPair(ref MinusC.y, ref PlusC.y, "ThrustY")) continue;
            if (st.LoadFloatPair(ref MinusC.z, ref PlusC.z, "ThrustZ")) continue;
            if (st.Recognize("Part")) { ADDCLASS<CRAFT_PART_DATA>(st); continue; }
            base.ProcessToken(st, value);
        } while (false);

    }
    public override void MakeLinks()
    {
        base.MakeLinks();
        if (Debris == null) stormdata_dll.StructError("CraftPart", FullName);

    }
    // data section
    public Vector3 RotateC;                  // угловой момент от этой части
    public Vector3 MinusC;                   // эффективность тяги в отрицательную сторону по осям
    public Vector3 PlusC;                    // эффективность тяги в положительную сторону по осям
    public Vector3 AeroC;                    // коэфф. эффетивности
    public float[] DamageXY = new float[4];
    public float[] DamageUV = new float[4];
};
