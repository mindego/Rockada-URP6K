using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

/*===========================================================================*\
|  Object - refferenced memory queries each Object *vftbl                     |
\*===========================================================================*/

public interface IObject : IRefMem
{
    //IID(0xADC31F52);
    new public const DWORD ID = 0xADC31F52;
    public object Query(uint value) { return null; }
    //public T Query<T>();
    //public T Query<T>() where T : IObject //TODO ¬озможно, такой каст работать не будет.
    //{
    //    return (T)Query(this.GetMyID());
    //}
    public virtual DWORD GetMyID() { return ID; }
    //public DWORD GetMyID();
    //template<class C> C* Query() { return (C*)Query(C::ID); }
};





