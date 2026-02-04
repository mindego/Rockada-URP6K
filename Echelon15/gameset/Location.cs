using LocationBase = GamesetMember<ILocation, IGamesetChange>;
using crc32 = System.UInt32;

using uvs = iUnifiedVariableString;
using uvv = iUnifiedVariableVector;
using uvi = iUnifiedVariableInt;
using uvb = iUnifiedVariableBlock;
using uvf = iUnifiedVariableFloat;
using uvr = iUnifiedVariableReference;
using uv = iUnifiedVariable;
using uvc = iUnifiedVariableContainer;
using uva = iUnifiedVariableArray;
using udb = iUnifiedVariableDB;
using tuvc = iUnifiedVariableContainer;
using tuva = iUnifiedVariableArray;
using tudb = iUnifiedVariableDB;

using System.Collections.Generic;
using UnityEngine;
//typedef GamesetMember<ILocation, IGamesetChange, IGamesetChange::onRenameLocation, IGamesetChange::onDeleteLocation, IGamesetChange::onChange> LocationBase;

//template <class RoadBuilderT, class RoadNetDataT, RoadBuilderT* createRb(IRoadsStore*, int)>
public class Location<RoadBuilderT, RoadNetDataT> : LocationBase, IGroupHolderChange, IRoadHolderChange, IRoadsStore, ILocation,ILoadableMember
{
    public const uint ID = 0x58171462;
    // IObject

    public ROADDATA getByCode(crc32 code, bool mustExists)
    {
        return myData.getRoadByCode(code, mustExists);
    }

    public override bool setName(string name)
    {
        bool ret = base.setName(name);
        if (ret)
            foreach (var i in myGroups)
            {
                i.Value.setChanged();
            }
        //for (int i = 0; i < myGroups.Count; i++)
        //{
        //    myGroups[i].setChanged();
        //}

        return ret;
    }

    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IStatus.ID: return getIStatus();
            case Location<RoadBuilderT, RoadNetDataT>.ID: return this;
            default: return null;
        }
    }

    // IGamesetMember 
    public virtual crc32 getID()
    {
        return Hasher.HshString(myName);
    }


    // IGroups
    public virtual IGroup getGroup(int i)
    {
        int j = 0;
        foreach (KeyValuePair<string, Group> group in myGroups)
        {
            if (i == j) return group.Value;
        }

        return null;
    }

    public virtual IGroup getGroupByName(string name)
    {
        if (!myGroups.TryGetValue(name, out Group group)) return null;
        return group;
        //crc32 hash_name = Hasher.HshString(name);
        //IGroup rec;

        //foreach (var group in myGroups)
        //{ }
        //    for (int i = 0; rec = getGroup(i); i++)
        //{
        //    if (Hasher.HshString(rec.getName()) == hash_name)
        //        return IRefMem.SafeAddRef(rec);
        //}
        //return null;
    }

    public virtual IGroup createGroup(string name)
    {
        myChange.onChange();
        return createGroupLocal(name);
    }

    // IMarkers
    public virtual IMarker getMarker(int i)
    {
        int j = 0;
        foreach (KeyValuePair<string, Marker> marker in myMarkers)
        {
            if (i == j) return marker.Value;
        }

        return null;
    }
    public virtual IMarker getMarkerByName(string name)
    {
        if (!myMarkers.TryGetValue(name, out Marker marker)) return null;
        return marker;
    }
    public virtual IMarker createMarker(string name)
    {
        myChange.onChange();
        return createMarkerLocal(name);
    }

    // IRoads
    public virtual IRoad createRoad(string name)
    {
        IRoad rd = createRoadLocal();
        if (rd != null)
        {
            rd.setName(name);
            myChange.onChange();
        }
        return rd;
    }

    public virtual IRoad getRoad(int i)
    {
        if (i >= myRoads.Count) return null;
        IRoad rec = myRoads[i];
        return IRefMem.SafeAddRef(rec);
    }
    // ILocation
    public virtual bool isChanged()
    {
        return (getStatus() != ChangeStatus.csNotChanged);
    }
    public virtual string getAiDlls() { return myAiDlls; }

    public virtual void setAiDlls(string dlls)
    {
        if (myAiDlls == dlls) return;
        myAiDlls = dlls;
        myChange.onChange();
        setChanged();
    }

    public virtual void setGameMapName(string name)
    {
        if (myGameMapName == name) return;
        myGameMapName = name;
        myChange.onChange();
        setChanged();
    }
    public virtual string getGameMapName() { return myGameMapName; }

    public virtual int getGameMapSizeX() { return mySizeX; }
    public virtual int getGameMapSizeZ() { return mySizeZ; }

    public virtual void setGameMapSizeX(int x)
    {
        if (x == mySizeX) return;
        mySizeX = x;
        myChange.onChange();
        setChanged();
    }
    public virtual void setGameMapSizeZ(int z)
    {
        if (z == mySizeZ) return;
        mySizeZ = z;
        myChange.onChange();
        setChanged();
    }

    public virtual string getMeMapName() { return myMeMapName; }
    public virtual void setMeMapName(string name)
    {
        if (myMeMapName == name) return;
        myMeMapName = name;
        myChange.onChange();
        setChanged();
    }

    public virtual void setTerrainName(string name)
    {
        if (myTerrain == name) return;
        myTerrain = name;
        myChange.onChange();
        setChanged();
    }
    public virtual string getTerrainName() { return myTerrain; }

    // IGroupHolderChange
    public virtual bool onRenameGroup(string name)
    {
        //return myGroups.canRename(name);
        return !myGroups.ContainsKey(name);
    }

    public virtual void onDeleteGroup(IGroup grp)
    {
        Group group = (Group)grp.Query(Group.ID);
        myGroups.Remove(group.getName());
        setChanged();
    }

    public virtual bool onRenameMarker(string name)
    {
        //return myMarkers.canRename(name);
        return !myGroups.ContainsKey(name);
    }
    public virtual void onChangeGroup()
    {
        myChange.onChange();
        setChanged();
    }
    public virtual void onChangeRoad()
    {
        myChange.onChange();
        setChanged();
    }
    public virtual void onChangeMarker()
    {
        myChange.onChange();
        setChanged();
    }
    public virtual void onDeleteMarker(IMarker mrk)
    {
        Marker tmpMarker = (Marker)mrk.Query(Marker.ID);
        myMarkers.Remove(tmpMarker.getName());
        setChanged();
    }

    // IRoadHolderChange 
    public virtual void onDeleteRoad(IRoad un)
    {
        RoadD tmpRoad = (RoadD)un.Query(RoadD.ID);
        myRoads.Remove(tmpRoad);
        myChange.onChange();
        setChanged();
    }

    // self

    public void loadRoadNet(bool load)
    {
        myLoadRoadNet = load;
    }

    public virtual IDataBlock getRoadNet()
    {
        Debug.Log("Trying to get myDataBlock " + myDataBlock + " " + myDataBlock.getLength());
        Asserts.Assert(myLoadRoadNet);
        //        return SafeAddRef(myDataBlock.Ptr());
        return myDataBlock;
    }

    RoadD createRoadLocal()
    {
        RoadD road = new RoadD(this, myData);
        myRoads.Add(road);
        return road;
    }

    bool save(uvc gsd)
    {
        gsd.setString("AiDlls", myAiDlls);
        gsd.setString("GameMapName", myGameMapName);
        gsd.setString("MeMapName", myMeMapName);
        gsd.setString("TerrainName", myTerrain);
        gsd.setInt("GameMapSizeX", mySizeX);
        gsd.setInt("GameMapSizeZ", mySizeZ);
        tuvc cont;

        cont = gsd.createContainer("Groups");
        Asserts.Assert(cont != null);
        Savings.saveSimple<Group, uvc>(cont, myGroups);

        cont = gsd.createContainer("Markers");
        Asserts.Assert(cont != null);
        Savings.saveSimple<Marker, uvv>(cont, myMarkers);

        tuva ar = gsd.createArray("Roads");
        Asserts.Assert(ar!=null);
        Savings.saveSimpleArray<RoadD, uvc, IRoadHolderChange>(ar, myRoads);

        // save bin array
        RoadBuilder roadBuilder = (RoadBuilder ) EnvironmentApi.CreateRB(this, IRoadBuilder.ROAD_BUILDER_VERSION);
        if (roadBuilder==null) return false;
        roadBuilder.MergeData(ar, null);

        RoadNetData net = roadBuilder.GetData();
        uvb block = gsd.createBlock("RoadNet");
        if (block==null)
        {
            roadBuilder.Release();
            return false;
        }

        block.SetValue(net.toArray(), net.FullSize());
        roadBuilder.Release();

        setNotChanged();

        saveNotify();

        return true;
    }
    public override bool load<UniType>(UniType gsd, ILoadErrorLog log = null)
    {
        return load((uvc) gsd, log);
    }
    bool load(uvc gsd, ILoadErrorLog log = null)
    {
        if (!Savings.loadStringVar("AiDlls", ref myAiDlls, gsd, log)) return false;
        if (!Savings.loadStringVar("GameMapName", ref myGameMapName, gsd, log)) return false;
        if (!Savings.loadStringVar("MeMapName", ref myMeMapName, gsd, log)) return false;
        if (!Savings.loadStringVar("TerrainName", ref myTerrain, gsd, log)) return false;
        if (!Savings.loadIntVar("GameMapSizeX", ref mySizeX, gsd, log)) return false;
        if (!Savings.loadIntVar("GameMapSizeZ", ref mySizeZ, gsd, log)) return false;
        tuvc cont;

        cont = gsd.createContainer("Groups");
        Asserts.Assert(cont!=null);
        Savings.enumSimple<Group, uvc, Location<RoadBuilderT,RoadNetDataT>>(cont, this, createGroupLocal, log);

        cont = gsd.createContainer("Markers");
        Asserts.Assert(cont!=null);
        Savings.enumSimple<Marker, uvv, Location<RoadBuilderT, RoadNetDataT>>(cont, this, createMarkerLocal, log);

        tuva ar = gsd.createArray("Roads");
        Asserts.Assert(ar!=null);
        Savings.enumSimpleArray<RoadD, uvc, Location<RoadBuilderT, RoadNetDataT>>(ar, this, createRoadLocal, log);

        setNotChanged();

        // form datablock
        if (myLoadRoadNet)
        {
            uvb block = gsd.openBlock("RoadNet");
            if (block!=null)
            {
                //SafeRelease(myDataBlock.Ptr());
                myDataBlock = new UnivarsDataBlock(block);
            }
            Debug.Log("RoadNet for " + myMeMapName + myDataBlock);
        }
        return true;
    }

    // creators
    Group createGroupLocal(string name)
    {
        Group grp = new Group(name, this, myData);
        myGroups.Add(name, grp);
        return grp;
    }

    Marker createMarkerLocal(string name)
    {
        Marker mrk = new Marker(name, this);
        myMarkers.Add(name, mrk);
        return mrk;
    }

    public bool renameNotify(string s)
    {
        throw new System.NotImplementedException();
    }


    public void changeNotify()
    {
        throw new System.NotImplementedException();
    }

    public void deleteNotify(IGamesetMember gsm)
    {
        throw new System.NotImplementedException();
    }

    ~Location()
    {
        int i = 0;
    }

    public Location(string name, IGamesetChange ch, IStormData data) : base(name, ch)
    {
        mySizeX = 0;
        mySizeZ = 0;
        myDataBlock = null;
        myData = data;
        myLoadRoadNet = false;
        setChanged();
    }

    string myTerrain;
    string myMeMapName;
    string myAiDlls;
    string myGameMapName;
    int mySizeX;
    int mySizeZ;
    IDataBlock myDataBlock;
    bool myLoadRoadNet;

    IStormData myData;

    //GenericMap<Group, IGroupHolderChange> myGroups;
    //GenericMap<Marker, IGroupHolderChange> myMarkers;
    Dictionary<string, Group> myGroups =new();
    Dictionary<string, Marker> myMarkers = new();
    //CommonMap<RoadD, IRoadHolderChange> myRoads;
    List<RoadD> myRoads = new List<RoadD>();
}