using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DWORD = System.UInt32;
using crc32 = System.UInt32;
using UnityEngine;

public class StormDataHolder<T> : List<T> where T : STORM_DATA
{
    string ItemName;

    public StormDataHolder(string itemName)
    {
        ItemName = itemName;
    }

    public string GetItemName()
    {
        return ItemName;
    }

    public T InsertItem(T item, int index)
    {
        Add(item);
        return item;
    }

    public void MakeLinks()
    {
        //TODO подумать, как реализовать правильно линкование в данных объектов.
        //throw new NotImplementedException();

        LinkingMessage(GetItemName());

        foreach (T t in this)
        {
            t.MakeLinks();
        }
    }

    void LoadingMessage(string ItemName)
    {
        Debug.Log(string.Format("Loading {0} datas...", ItemName));
    }

    void LinkingMessage(string ItemName)
    {
        Debug.Log(string.Format("Linking {0} datas...", ItemName));
        //LogMessage("Linking {0} datas...", ItemName);
    }

    public T GetItem(string name)
    {
        var hash = Hasher.HshString(name);
        foreach (var z in this)
        {
            if (z.Name == hash) return z;
        }
        return null;
    }

    public T GetItem(string n, uint c, bool me)
    {
        if (n != null) c = Hasher.HashString(n);
        foreach (var z in this)
        {
            if (z.Name == c) return z;
        }
        
        if (me) stormdata_dll.CantFindError(ItemName, n, c);
        return null;
    }
}
public class OBJECT_DATA : STORM_DATA, iSTORM_DATA<OBJECT_DATA>
{
    public static StormDataHolder<OBJECT_DATA> Datas = new StormDataHolder<OBJECT_DATA>("object");
    // маски
    public const uint OF_CLASS_MASK = 0x000000FF;
    public const uint OF_FLAGS_MASK = 0x0000FF00;
    // классы подобъектов =
    public const uint OC_STATIC = 0x00000010;
    public const uint OC_HANGAR = 0x00000011;
    public const uint OC_SFG = 0x00000012;
    public const uint OC_VEHICLE = 0x00000020;
    public const uint OC_CRAFT = 0x00000040;
    public const uint OC_AIRSHIP = 0x00000080;
    public const uint OC_SEASHIP = 0x00000081;

    public string Description;
    public string DescriptionShort;
    public uint Flags;
    //public CampaignDefines.SideTable Side; //int
    public int Side;
    public int UnitDataIndex;
    public float SensorsRange;
    public float SensorsVisibility;
    public float CriticalDamagedTime;
    public bool SpawnShadow;
    public SUBOBJ_DATA RootData;
    //TLIST<LAYOUT_DATA> Layouts;
    public List<LAYOUT_DATA> Layouts = new List<LAYOUT_DATA>();
    //Tab<AnimationPackage*> myAnimations;
    public List<AnimationPackage> myAnimations = new List<AnimationPackage>();
    //Tab<crc32> myLinkedSubObj;
    public List<crc32> myLinkedSubObj = new List<crc32>();
    public OBJECT_DATA pOtherSideData;
    //public string pOtherSideData;
    public tmpOBJECT_DATA lateInitstage = new tmpOBJECT_DATA();

    public string[] SideTable = { "HUMANS", "VELIANS", "NEUTRAL", "ALIENS" };
    // data access
    public void SetFlag(DWORD Flag) { Flags |= Flag; }
    public void ClearFlag(DWORD Flag) { Flags &= ~Flag; }
    DWORD GetFlag(DWORD Flag) { return (Flags & Flag); }
    public DWORD GetClass() { return (Flags & OF_CLASS_MASK); }

    public string GetClassName()
    {
        DWORD ClassId = GetClass();
        Dictionary<DWORD, string> classes = new Dictionary<DWORD, string>();
        classes.Add(OC_STATIC, "OC_STATIC");
        classes.Add(OC_HANGAR, "OC_HANGAR");
        classes.Add(OC_SFG, "OC_SFG");
        classes.Add(OC_VEHICLE, "OC_VEHICLE");
        classes.Add(OC_CRAFT, "OC_CRAFT");
        classes.Add(OC_AIRSHIP, "OC_AIRSHIP");
        classes.Add(OC_SEASHIP, "OC_SEASHIP");
        if (classes.ContainsKey(ClassId)) return classes[ClassId];
        return "UNKNOWN";
    }
    public bool IsClass(DWORD Flag) { return ((Flags & Flag) == 0) ? false : true; }

    public OBJECT_DATA() : this("undefined") { }
    public OBJECT_DATA(string name) : base(name)
    {
        //Description = AddString(gpDescriptionCtr, Name);
        //DescriptionShort = AddString(gpDescriptionShortCtr, Name);
        Description = "No description";
        DescriptionShort = "No description";
        Flags = 0;
        Side = 0;
        SpawnShadow = true;
        UnitDataIndex = -1;
        SensorsRange = 2000f;
        SensorsVisibility = 1f;
        CriticalDamagedTime = 1f;
        RootData = null;
        pOtherSideData = null;
    }

    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            //Перенесено на этап линкования
            //if (st.LdHST(ref pOtherSideData, "OtherSideData")) continue;

            if (st.Load_String(ref lateInitstage.OtherSideData, "OtherSideData")) continue;

            if (st.LoadIndexFromTable(ref Side, "Side", SideTable))
            {
                StormLog.LogMessage("Side is: " + Side);
                //continue;
                return;
            }
            if (st.Recognize("Root"))
            {

                RootData = new SUBOBJ_DATA(st.GetNextItem());
                RootData.Load(st);
                continue;
            }
            if (st.Recognize("UnitData"))
            {
                UnitDataIndex = UnitDataTable.GetIdxByName(st.GetNextItem());
                if (UnitDataIndex < 0) throw new Exception("Unit not found in UnitDataTable");
                SensorsRange = UnitDataTable.pUnitDataTable.GetUD(UnitDataIndex).SensorRadius;
                SensorsVisibility = UnitDataTable.pUnitDataTable.GetUD(UnitDataIndex).SensorVisibility;
                return;
            }
            if (st.LoadBool(ref SpawnShadow, "Shadow")) continue;
            if (st.LoadFloat(ref SensorsRange, "SensorsRange")) continue;
            if (st.LoadFloat(ref SensorsVisibility, "SensorsVisibility")) continue;
            if (st.LoadFloat(ref CriticalDamagedTime, "CriticalDamagedTime")) continue;
            if (st.Recognize("Layout"))
            {
                LAYOUT_DATA l = new LAYOUT_DATA();
                l.Init(st);
                Layouts.Add(l);
                continue;
            }
            if (st.Recognize("Reference"))
            {
                Reference(GetByName(st.GetNextItem()));
                return;
            }

            if (st.Recognize("Animation"))
            {
                string name = st.GetNextItem();
                AnimationPackage data = AnimationPackage.getAnimationPackage(Hasher.HashString(name));
                if (data != null)
                {
                    myAnimations.Add(data);
                    continue;
                }
                //PARSE_ERROR;
                throw new System.Exception(string.Format("{0} {1} {2}", st.LineNumber(), "Animations", st.DebugStream()));
            }
            // load linked subobjects
            if (st.Recognize("LinkedSubobj")) { myLinkedSubObj.Add(Hasher.HshString(st.GetNextItem())); continue; }

            base.ProcessToken(st, value);
        } while (false);
    }

    public struct tmpOBJECT_DATA
    {
        public string OtherSideData;
    }
    public virtual void Reference(OBJECT_DATA data)
    {
        Flags = data.Flags;
        Side = data.Side;
        SpawnShadow = data.SpawnShadow;
        UnitDataIndex = data.UnitDataIndex;
        SensorsRange = data.SensorsRange;
        SensorsVisibility = data.SensorsVisibility;
        CriticalDamagedTime = data.CriticalDamagedTime;
        pOtherSideData = data.pOtherSideData;
    }

    public override void MakeLinks()
    {
        if (RootData == null) stormdata_dll.StructError("object", FullName);
        RootData.SetFlag(SUBOBJ_DATA.SF_CRITICAL);
        RootData.MakeLinks();
        if (UnitDataIndex < 0) stormdata_dll.StructError("object", FullName);
        StormDataHDR.RSLV<OBJECT_DATA>(ref pOtherSideData);
    }

    public static OBJECT_DATA GetByName(string Name, bool MustExist = true)
    {
        //return Datas.GetItem(Name, 0, MustExist);
        uint Code = Constants.THANDLE_INVALID;
        if (Name != "") Code = Hasher.HshString(Name);
        //if (Name != "") Code = Hasher.CodeString(Name);
        return GetByCode(Code, MustExist);
    }

    public static OBJECT_DATA GetByCode(DWORD Code, bool MustExist = true)
    {
        //return Datas.GetItem(Name, 0, MustExist);
        //uint Code = Constants.THANDLE_INVALID;
        //if (Name != "") Code = Hasher.HshString(Name);
        foreach (OBJECT_DATA data in Datas)
        {
            //Debug.Log(string.Format("Matching {0} vs {1} for {2} in {3}", Code.ToString("X8"), data.Name.ToString("X8"), data.FullName, Datas.Count));
            if (data.Name == Code) return data;
        }
        if (MustExist) throw new System.Exception($"Could not get OBJECT_DATA {Code:X8}");
        return null;
    }

    public static OBJECT_DATA InsertObjectData(OBJECT_DATA d, int l)
    {
        OBJECT_DATA.Datas.Add(d);
        return d;
    }

    public LAYOUT_DATA GetLayout(DWORD Type, string LayoutName)
    {
        return GetLayout(Type, Hasher.HshString(LayoutName));
    }
    public LAYOUT_DATA GetLayout(DWORD Type, DWORD Code = 0xFFFFFFFF)
    {
        //for (LAYOUT_DATA l = Layouts.Head(); l; l=l->Next())
        //        if (l->Type==Type && (Code==0xFFFFFFFF || l->Name==Code))
        //                break;
        //        return l;

        foreach (LAYOUT_DATA l in Layouts)
        {
            //Debug.Log(string.Format("Processing layout {0} name {1}",l.FullName,l.Name.ToString("X8")));
            if ((l.Type == Type) && (Code == 0xFFFFFFFF || l.Name == Code)) return l;
        }
        Debug.Log(string.Format("LAYOUT_DATA get failed Type [{0}] Code [{1}] of {2}", Type.ToString("X8"), Code.ToString("X8"), Layouts.Count));
        if (Layouts.Count == 0) return null;
        return Layouts[0] != null ? Layouts[0] : null;
    }

    public static void loadObjectData(int code, IMappedDb db)
    {
        switch (code)
        {
            case 0:
                STATIC_DATA.loadStaticData(db);
                //HANGAR_DATA.loadHangarData(db); //TODO! Объединить загрузку ОШ и СН. Эта строка нужна только для ОШ, т.к. в ОШ ангары отдельно прописаны как Hangar("name") в gdata, а в СН это Static, у которого есть субобъект с hangar
                VEHICLE_DATA.loadVehicleData(db);
                CRAFT_DATA.loadCraftData(db);
                CARRIER_DATA.loadCarrierData(db);
                return;
            case 1:
                Datas.MakeLinks();
                return;
        }
    }
    public override string ToString()
    {
        string res = base.ToString();
        foreach (LAYOUT_DATA layout in Layouts)
        {
            res += "\n" + layout;
        }

        return res;
        //return this.GetType() + " " + FullName + "\n" + Name + ":" + (int)Name + " " + Name.ToString("X8");
    }

    public OBJECT_DATA GetByCodeLocal(uint Code, bool MustExist = true)
    {
        return GetByCode(Code, MustExist);
    }
}