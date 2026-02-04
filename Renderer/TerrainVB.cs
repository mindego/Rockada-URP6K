using UnityEngine;
using static dwFlags;
//using NonHashList = System.Collections.Generic.List<IHashObject>;
//using NonHashList = System.Collections.Generic.List<HashObjectCont>;
public class TerrainVB : Invalidator
{
    public VBuffer vbuffer;
    StormTerrain invalidate_hook;
    bool locked;

    public TerrainVB()
    {
        vbuffer = null;
        locked = false;
        invalidate_hook = null;
    }
    ~TerrainVB() { Dispose(); }
    public void Dispose()
    {
        IMemory.SafeRelease(vbuffer);
    }

    public void Lock()
    {
        if (!locked)
        {
            try
            {
                vbuffer.Lock((uint)DDLOCK_WAIT | (uint)DDLOCK_WRITEONLY | (uint)DDLOCK_NOOVERWRITE);
            }
            catch
            {
                Debug.LogFormat("Failed to lock vbuffer {0} for TerrainVB {1}", vbuffer == null ? "null" : vbuffer.GetHashCode().ToString("X8"), this.GetHashCode().ToString("X8"));
            }
            locked = true;
        }

    }

    public void Unlock()
    {
        if (locked)
        {
            vbuffer.Unlock();
            locked = false;
        }
    }

    public void setInvalidateHook(StormTerrain hook)
    {
        invalidate_hook = hook;
    }

    public void setVBuffer(VBuffer vb)
    {
        Debug.LogFormat("setVBuffer {0} {1} for {2}", vb, vb.GetHashCode().ToString("X8"), this.GetHashCode().ToString("X8"));
        vbuffer = vb;
        vbuffer.setInvalidateHook(this);
        Debug.LogFormat("setVBuffer {0} {1} success for {2}", vb, vb.GetHashCode().ToString("X8"), this.GetHashCode().ToString("X8"));
    }

    public void invalidate()
    {
        invalidate_hook.Invalidate();
    }
};
