using DWORD = System.UInt32;
using static HudData;
using UnityEngine;
using System;

//public class RecticleData : TLIST_ELEM<RecticleData>
public class HudRecticleData : TLIST_ELEM_IMP<HudRecticleData>,IDisposable
{
    public enum TYPE
    {
        GUNSIGHT, RING, SIMPLETARGET,
        CURRENTTARGET, ANIMETARGET, CURRENTWPT, ANIMEWPT, X,
        MISSILECONTROL, LAMP_A, LAMP_T, LAMP_O, LAMP_M, ARROW, ANIMEARROW, ROMB
    };

    public DWORD UID;
    public float x, y, stage;
    public float percent;
    //char text[256];
    //char textup[256];
    public string text;
    public string textup;
    public DWORD colour;
    public float opacity, w, h;

    public TYPE Type;
    public bool hide;

    public IHUDObject Dev;
    IHUDObject DevOut;

    public HudRecticleData(TYPE _Type, float _w, float _h, DWORD _c, DWORD uid)
    {
        Type = _Type;
        x = (0);
        y = (0);
        Dev = null;
        DevOut = null;
        hide = (false);
        colour = (_c);
        UID = (uid);
        opacity = 1;
        w = _w;
        h = _h;
        textup = null;
        text = null;
    }

    public void SetPercent(float p)
    {
        percent = p;
    }
    public void setRadius(float r)
    {
        
        w = h = r;
    }
    public void SetColor(int c)
    {
        colour = (uint)c;
    }
    public void Hide(bool h)
    {
        hide = h;
    }
    public void SetOpacity(float o)
    {
        opacity = o;
        //if (opacity < FLT_EPSILON) hide = true;
        if (opacity < 0) hide = true;
        else hide = false;
    }

    //public RecticleData Next()
    //{
    //    throw new System.NotImplementedException();
    //}

    //public RecticleData Prev()
    //{
    //    throw new System.NotImplementedException();
    //}

    //public void SetNext(RecticleData t)
    //{
    //    throw new System.NotImplementedException();
    //}

    //public void SetPrev(RecticleData t)
    //{
    //    throw new System.NotImplementedException();
    //}


    public void Dispose()
    {
        if (Dev != null) Dev.GetTree().Sub();
        if (DevOut != null) DevOut.GetTree().Sub();
    }
    ~HudRecticleData()
    {
        Dispose();
    }
}
