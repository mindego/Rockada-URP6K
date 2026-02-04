using static LightDefines;
using static LIGHT_PRIORITY;
using UnityEngine.Assertions;
using static HashFlags;
using static RoFlags;
using static CppFunctionEmulator;
using static renderer_dll;
using static D3DEMULATION;
/// <summary>
/// Константы для света. В оригинале - часть фавла direct3dlights.hpp
/// </summary>
public static class LightDefines
{
    public const uint LP_SHIFT = 16;
    public const uint LP_MASK = 0xffff0000;

    public const uint lsPASSIVE = 0x00000001;
    public const uint lsACTIVE = 0x00000002;
    public const uint lsTO_ACTIVATE = 0x00000004;
    public const uint lsUPDATED = 0x00000008;
    public const uint LS_SHIFT = 0;
    public const uint LS_MASK = 0xffff;


    public const uint LS_ENABLED = 0x80000000;

    public const uint LR_OK = 0x00000000;
    public const uint LR_CANTADD = 0x00000001;
    public const uint LR_CANTENABLE = 0x00000002;
}
public class LightAdder : HashEnumer
{
    LightManager lightm;
    LIGHT[] lights;
    public int numlights;

    public LightAdder(LightManager _lightm, LIGHT[] _lights)
    {
        lightm = _lightm;
        lights = _lights;
        numlights = 0;
    }
    public bool ProcessElement(HMember h)
    {
        ILightEss light = (ILightEss)h.Object().Query(ILightEss.ID);
        lights[numlights++] = light.GetLIGHT();
        light.Release();
        return true;
    }
}

public class LightManager
{
    int Max;
    int MaxActive;
    int frame_counter;
    int num_active;
    int num_free;
    int num_added;
    int num_updates;
    int num_allactive;
    LightIndex[] AllLights;
    LightIndex[] FreeStack;

    LightIndex[] AddedList = new LightIndex[(int)lpENUM_SIZE];
    LightIndex[] ActiveList = new LightIndex[(int)lpENUM_SIZE];
    LightIndex[] ToActivateList = new LightIndex[(int)lpENUM_SIZE];
    LightIndex[] ToDeActivateList = new LightIndex[(int)lpENUM_SIZE];

    int Add(LIGHT light, LIGHT_PRIORITY priority)
    {
        if (num_added >= Max) return false ? 1 : 0; //Да, отдаёт индийским кодом. Но так - минимум изменений в оригинальный код.

        num_added++;
        LightIndex next = AddedList[(int)priority];
        LightIndex list;
        AddedList[(int)priority] = list = FreeStack[--num_free];
        list.Add((int)priority, (int)lsPASSIVE, light, next);
        light.id.AddRef(list, frame_counter);
        light.id.index.ClearStatus(lsUPDATED);

        return true ? 1 : 0;
    }

    int Activate(LightIndex idx, int priority)
    {
#warning("think of light priority")
        HRESULT hr;

        if (num_active >= MaxActive) return false ? 1 : 0;

        if ((idx.GetStatus() & lsUPDATED) == 0)
        {
            D3DLIGHT7 l = idx.light.GetD3DLight().d3d_desc;
            hr =
            d3d.Device().SetLight(idx.d3dindex, ref l);
            Assert.IsTrue(SUCCEEDED(hr));
            idx.SetStatus(lsUPDATED);
        }


        num_active++;

        if ((idx.GetStatus() & lsACTIVE) != 0) return true ? 1 : 0;

        hr =
        d3d.Device().LightEnable(idx.d3dindex, true);
        Assert.IsTrue(SUCCEEDED(hr));

        LightIndex next = ActiveList[priority];
        Assert.IsTrue(idx != next);

        ActiveList[priority] = idx;
        idx.next_active = next;
        idx.ClearStatus(lsTO_ACTIVATE);
        idx.SetStatus(lsACTIVE);
        return true ? 1 : 0;
    }
    int DeActivate(LightIndex idx, int priority)
    {
        num_active--;
        d3d.Device().LightEnable(idx.d3dindex, false);
        idx.ClearStatus(lsACTIVE);
        idx.SetStatus(lsPASSIVE);
        return true ? 1 : 0;
    }

    public int Create(int MaxLights, int MaxActiveLights)
    {
        Max = MaxLights;
        MaxActive = MaxActiveLights;

        AllLights = new LightIndex[Max];
        FreeStack = new LightIndex[Max];

        for (int i = 0; i < Max; ++i)
        {
            AllLights[i] = new LightIndex();
            FreeStack[i] = AllLights[i];
            FreeStack[i].d3dindex = Max - i - 1;
        }

        frame_counter = 0;
        return 0;
    }

    int Destroy()
    {
        //TODO - возможно тут правильнее удалять содержимое массивов перед тем как удалять их самих.
        if (FreeStack != null) { FreeStack = null; }
        if (AllLights != null) { AllLights = null; }
        return 0;
    }

    int StartFrame()
    {
        num_free = Max;
        num_added = 0;
        num_active = 0;
        num_updates = 0;
        num_allactive = 0;
        for (int i = 0; i < (int)lpENUM_SIZE; ++i)
        {
            AddedList[i] = null;
            ActiveList[i] = null;
            ToActivateList[i] = null;
            ToDeActivateList[i] = null;
        }
        frame_counter++;
        return 0;
    }
    int EndFrame()
    {
        for (int i = 0; i < (int)lpENUM_SIZE; ++i)
        {
            LightIndex l = ToActivateList[i];
            for (; l!=null; l = l.next_active)
            {
                ToDeActivate(l.light, (LIGHT_PRIORITY)i);
            }
            ToActivateList[i] = null;
        }

        UpdateActivity();

        //dprintf( " Free Lights %d, Active %d , AverageActive %4.4f ( %d/%d) \n", num_free, num_active, num_allactive/float(num_updates),num_allactive,num_updates );
        return true ? 1:0;
    }

    public int ToActivate(LIGHT light, LIGHT_PRIORITY priority)
    {
#if _DEBUG
                  for( int i=0;i<lpENUM_SIZE;++i ) {
                    LightIndex *l=ToActivateList[i]; 
                    if(priority==i) continue;
                    for ( ;l;l=l.next_toactivate ) {
                      AssertBp(l.light!=light);
                    }
                  }

#endif

        if (!light.id.UpToDate(frame_counter))
            if (Add(light, priority) == 0) return 0;

        LightIndex idx = light.id.index;
        if ((idx.GetStatus() & lsTO_ACTIVATE) != 0) return true ? 1 : 0;
        LightIndex next = ToActivateList[(int)priority];
        ToActivateList[(int)priority] = idx;
        Assert.IsTrue(next != idx);
        idx.next_toactivate = next;
        idx.SetStatus(lsTO_ACTIVATE);
        return true ? 1 : 0;
    }
    public int ToDeActivate(LIGHT light, LIGHT_PRIORITY priority)
    {
        LightIndex idx = light.id.index;
        idx.ClearStatus(lsTO_ACTIVATE);
        return true ? 1 : 0;
    }

    int UpdateActivity()
    {
        int i;
        for (i = 0; i < (int)lpENUM_SIZE; ++i)
        {
            LightIndex l = ActiveList[i];
            LightIndex prev = null;

            for (; l != null;)
            {
                if ((l.GetStatus() & lsTO_ACTIVATE) == 0)
                {
                    DeActivate(l, i);
                    if (prev != null)
                    {
                        l = (prev.next_active = l.next_active);
                    }
                    else
                    {
                        l = ActiveList[i] = l.next_active;
                    }
                }
                else
                {
                    prev = l;
                    l = l.next_active;
                }
            }
        }
        for (i = 0; i < (int)lpENUM_SIZE; ++i)
        {
            LightIndex l = ToActivateList[i];
            LightIndex prev = null;
            for (; l != null; l = l.next_toactivate)
            {
                if ((l.GetStatus() & lsACTIVE) == 0)
                    Activate(l, i);
                l.ClearStatus(lsTO_ACTIVATE);
                prev = l;
            }
            ToActivateList[i] = null;
        }
        num_allactive += num_active;
        num_updates++;
        return true ? 1 : 0;
    }
}

public enum LIGHT_PRIORITY
{
    lpGLOBAL,
    lpMEDIAN,
    lpLOCAL,
    lpENUM_SIZE,
    lpFORCE_DWORD = 0x7fffffff
};

struct LightActivator : HashEnumer
{
    LightManager lightm;

    public LightActivator(LightManager _lightm) { lightm = _lightm; }
    public bool ProcessElement(HMember h)
    {
        Assert.IsTrue(h.Object().MatchFlags(ROObjectId(ROFID_LIGHT)));
        ILightEss light = (ILightEss)h.Object().Query(ILightEss.ID);
        lightm.ToActivate(light.GetLIGHT(), lpLOCAL);
        light.Release();
        return true;
    }
};

struct LightDeActivator : HashEnumer
{
    LightManager lightm;

    LightDeActivator(LightManager _lightm) { lightm = _lightm; }
    public bool ProcessElement(HMember h)
    {
        Assert.IsTrue((h.Object().MatchFlags(ROObjectId(ROFID_LIGHT))));
        ILightEss light = (ILightEss)h.Object().Query(ILightEss.ID);
        lightm.ToDeActivate(light.GetLIGHT(), lpLOCAL);
        light.Release();
        return true;
    }
};

public struct D3DLightData
{
    public D3DLIGHT7 d3d_desc;
};