using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EXPLOSION_INFO
{
    public static float EXPLOSION_CUT_RADIUS =0.1f;
    public EXPLOSION_DATA explosion;
    public float XDamage, XRadius;

    public override string ToString()
    {
        return string.Format("EXPLOSION_INFO XDamage {0} XRadius {1} EXPLOSION_DATA {2}", XDamage,XRadius,explosion);
    }
    public EXPLOSION_INFO()
    {
        Zero();
    }
    public EXPLOSION_INFO(EXPLOSION_INFO reference)
    {
        explosion = reference.explosion;
        XDamage = reference.XDamage;
        XRadius = reference.XRadius;

    }
    public void Set(float xDamage, float xRadius) { XDamage = xDamage; XRadius = xRadius; }
    public void Zero() { explosion = null ; XDamage = XRadius = 0; }
    public void Load(READ_TEXT_STREAM st)
    {
        string c = st.GetNextItem();
        if (c != "{") return;
        c = st.GetNextItem();
        if (c == "}") return;
        //explosion = (EXPLOSION_DATA) Hasher.HshString(c);
        //explosion = null; //TODO: Исправить на правильное
        //Debug.Log("ExplInfo: " + c);
        explosion = EXPLOSION_DATA.Datas.GetItem(c);
        //Debug.Log(explosion);
        c = st.GetNextItem();
        if (c == "}") return;
        XRadius = st.AtoF(c);
        c = st.GetNextItem();
        if (c == "}") return;
        XDamage = st.AtoF(c);
        c = st.GetNextItem();
        while (c!=null && c != "}") c = st.GetNextItem();

    }
    public void MakeLinks()
    {
        StormDataHDR.RSLV<EXPLOSION_DATA>(ref explosion);
        if (XRadius!=0) XDamage /= Mathf.Pow(XRadius,2);

    }

    public static EXPLOSION_INFO SafeExplosionInfoLoad(EXPLOSION_INFO old, READ_TEXT_STREAM st, float XD, float XR)
    {
        SafeExplosionInfoDelete(old);
        EXPLOSION_INFO info = new EXPLOSION_INFO();
        info.Set(XD, XR);    // set default
        info.Load(st);
        return info;

    }
    public static void SafeExplosionInfoMakeLinks(EXPLOSION_INFO info,string s)
    {
        if (info!=null)
        {
            info.MakeLinks();
            if (info.explosion == null)
            {
                StormLog.LogMessage($"Warning : explosion \"{s}\" not finded, deleted.");
                //delete(*info);
                info = null;
            }
        }

    }
    public static EXPLOSION_INFO SafeExplosionInfoCopy(EXPLOSION_INFO info)
    {
        if (info!=null)
            return new EXPLOSION_INFO(info);
        return null;

    }
    public static void SafeExplosionInfoDelete(EXPLOSION_INFO info)
    {
        if (info != null) StormLog.LogMessage("Deleting " + info);
            //delete info;

    }
}
