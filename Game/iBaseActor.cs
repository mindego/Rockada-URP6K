using System;
using UnityEngine;

public interface iBaseActor : iBaseInterface,IDisposable//TLIST_ELEM<iBaseActor>
{
    public const uint ID = 0x33FCBA4A;
    //public bool Move(float scale) { Debug.Log("Interface Move"); return false; }
    //public void Update(float scale) { Debug.Log("Interface Update:" + this); }
    public bool Move(float scale);
    public void Update(float scale);

    public void BaseActorInit(BaseScene s);
    public void BaseActorDispose();

    public BaseScene rScene
    {
        get;
    }
}

