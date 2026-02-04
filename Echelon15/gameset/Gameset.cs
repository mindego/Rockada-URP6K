using System.Collections.Generic;
using uvc = iUnifiedVariableContainer;
using tuvc = iUnifiedVariableContainer;
using uvs = iUnifiedVariableString;
using System;
using UnityEngine;
//using GSLocation = Location<RoadBuilderT, RoadNetDataT>;
//typedef Location<RoadBuilderT, RoadNetDataT, createRb> GSLocation;

//template <class RoadBuilderT, class RoadNetDataT, RoadBuilderT* createRb(IRoadsStore*, int)>
public class Gameset<RoadBuilderT, RoadNetDataT> : Status, IGameset, IGamesetChange, IMessageChange
{
    // creators
    Record createRecordLocal(string name)
    {
        if (!myMissions.ContainsKey(name) && !mySelectionEvents.ContainsKey(name))
        {
            Record rec = new Record(name, this);
            myRecords.Add(name, rec);
            return rec;
        }
        return null;
    }
    SelectionEvent createSelectionEventLocal(string name)
    {
        if (!myMissions.ContainsKey(name) && !myRecords.ContainsKey(name))
        {
            SelectionEvent se = new SelectionEvent(name, this);
            mySelectionEvents.Add(name, se);
            return se;
        }
        return null;
    }
    // creators
    public bool isChanged()
    {
        return myStatus != ChangeStatus.csNotChanged;
    }

    private void debug() { }
    Mission createMissionLocal(string name)
    {
        if (!myRecords.ContainsKey(name) && !mySelectionEvents.ContainsKey(name))
        {
            Mission msn = new Mission(name, this, myData);
            myMissions.Add(name, msn);
            return msn;
        }
        return null;
    }

    Message createMessageLocal(string name)
    {
        Message msg = new Message(name, this);
        //myMessages.Add(name, msg);
        if (!myMessages.TryAdd(name, msg)) throw new Exception(string.Format("Already has record with name [{0}]",name));
        return msg;
    }

    //typedef Location<RoadBuilderT, RoadNetDataT, createRb> GSLocation;
    Location<RoadBuilderT, RoadNetDataT> createLocationLocal(string name)
    {
        if (myExclusiveLocationName != null && myExclusiveLocationName != name)
            return null;
        var loc = new Location<RoadBuilderT, RoadNetDataT>(name, this, myData);
        Debug.Log("Adding location " + loc.getGameMapName() + " " + name);
        myLocations.Add(name, loc);
        if (loc != null)
            loc.loadRoadNet(myExclusiveLocationName!=null);

        return loc;
    }

    // enumerators
    void getEvents(uvc root1, uvc root2, ILoadErrorLog log, string msn_name)
    {
        uint handle = 0;
        if (root1 == null) log.addWarning(0,"","Empty root1");

        while ((handle = root1.GetNextHandle(handle)) != 0)
        {
            tuvc gsd_ev = root1.openContainer(handle);
            if (gsd_ev != null)
            {
                string name = gsd_ev.getNameShort();
                //Debug.Log("Loading [" + name + "] from data " + gsd_ev + " Mission " + msn_name);

                if (msn_name != null && msn_name != name) continue;
                // load from game => store UDB path to mission & location
                if (msn_name != null)
                {
                    Debug.Log("Loading [" + name + "] from data:\n" + gsd_ev + " Mission " + msn_name);
                    myOnlyMission = gsd_ev;
                    myOnlyLocation = gsd_ev.openContainer("Location");
                }
                // load from game => store UDB path to mission & location
                tuvc gsl_ev = root2.openContainer(name);
                if (gsd_ev != null && gsl_ev != null)
                {
                    uint type;
                    gsd_ev.getInt("Type", out type);
                    switch (type)
                    {
                        case 0x63676558: //Record
                            Savings.loadTwin<Record, uvc, Gameset<RoadBuilderT,RoadNetDataT>>(name, gsd_ev, gsl_ev, this, createRecordLocal, log);
                            break;
                        case 0xA025345F: //Mission
                            Savings.loadTwin<Mission, uvc, Gameset<RoadBuilderT, RoadNetDataT>>(name, gsd_ev, gsl_ev, this, createMissionLocal, log);
                            break;
                        case 0xEBAB718B: //SelectionEvent
                            Savings.loadTwin<SelectionEvent, uvc, Gameset<RoadBuilderT, RoadNetDataT>>(name, gsd_ev, gsl_ev, this, createSelectionEventLocal, log);
                            break;
                    }
                }
                else if (log != null)
                    log.addWarning(DataCore.LE_LOCALIZATION_FOR_EVENT_MISSED, name, "skipped");
            }
        }
    }

    string getLocationForSingleMission()
    {
        IMission msn = getMission(0);
        return msn != null ? msn.getLocation() : null;
    }

    bool loadData(ILoadErrorLog log, string msn_name)
    {
        //Debug.Log(myGsd.GetRoot());
        //var val1 = myGsd.GetRoot().GetVariableByName("\\Root\\Locations\\Continent", 0xFFFFFFFF);
        ////Debug.Log("\\Root\\Locations\\Continent".IndexOf('\\'));
        //Debug.Log("Long path res " + (val1!=null ? val1:null));
        //var val2 = myGsd.GetRoot().GetVariableByName("Events", 0xFFFFFFFF);
        //Debug.Log("Short path res " + val2);
        //throw new Exception("failsage");
        // self
        //StormLog.defaultPriority = StormLog.logPriority.DEBUG;

        myGsl.GetRoot().getString("Title", ref myTitle);
        Debug.Log("Loaded title: " + myTitle );
        myGsl.GetRoot().getString("Description", ref myDesc);
        Debug.Log("Loaded Description: " + myDesc);// + " root " + myGsl.GetRoot()) 
        UnivarDBHelper.getStringArray(myGsl.GetRoot(), "Voice Files", ref myVoices, GamesetDefines.gMaxRadioVoices);

        tuvc cont1, cont2;

        // records
        cont1 = myGsd.GetRoot().createContainer("Events");
        cont2 = myGsl.GetRoot().createContainer("Events");
        //Debug.Log("Root name: " + myGsl.GetRoot().getNameShort());
        //Debug.Log("Container name: " + cont2.getNameShort());
        getEvents(cont1, cont2, log, msn_name);
        //throw new SystemException("Stop here to rid of stack overflow");

        if (msn_name != null)
            myExclusiveLocationName = getLocationForSingleMission();

        cont2 = myGsl.GetRoot().createContainer("Game Messages");
        
        Savings.enumSimple<Message, uvs, Gameset<RoadBuilderT, RoadNetDataT>>(cont2, this, createMessageLocal, log);

        cont1 = myGsd.GetRoot().createContainer("Locations");
        //Debug.Log(cont1);
        Savings.enumSimple<Location<RoadBuilderT, RoadNetDataT>, uvc, Gameset<RoadBuilderT, RoadNetDataT>>(cont1, this, createLocationLocal, log);

        //bool success = msn_name==null || myLocations.getSize()!=0;
        bool success = msn_name == null || myLocations.Count != 0;

        setNotChanged();
        myOwnChanges = false;

        return success;
    }

    void saveMissionInt(string name)
    {
        Asserts.Assert(myExclusiveLocationName == null);
        tuvc cont1, cont2;
        cont1 = myGsd.GetRoot().createContainer("Events");
        cont2 = myGsl.GetRoot().createContainer("Events");
        Savings.saveTwin<Mission, uvc>(cont1, cont2, myMissions, name);
    }

    void saveRecordInt(string name)
    {
        Asserts.Assert(myExclusiveLocationName == null);
        tuvc cont1, cont2;
        cont1 = myGsd.GetRoot().createContainer("Events");
        cont2 = myGsl.GetRoot().createContainer("Events");
        Savings.saveTwin<Record, uvc>(cont1, cont2, myRecords, name);
    }
    void saveSelectionEventInt(string name)
    {
        Asserts.Assert(myExclusiveLocationName==null);
        tuvc cont1, cont2;
        cont1 = myGsd.GetRoot().createContainer("Events");
        cont2 = myGsl.GetRoot().createContainer("Events");
        Savings.saveTwin<SelectionEvent, uvc>(cont1, cont2, mySelectionEvents, name);
    }

    void saveLocationInt(string name)
    {
        Asserts.Assert(myExclusiveLocationName == null);
        tuvc cont1;
        cont1 = myGsd.GetRoot().createContainer("Locations");
        Savings.saveSimple<Location<RoadBuilderT, RoadNetDataT>, uvc>(cont1, myLocations, name);
    }


    bool getGamesetChanges()
    {
        foreach (var msn in myMissions)
        {
            if (msn.Value.isChanged()) return true;
        }
        foreach(var rec in myRecords)
        {
            if (rec.Value.isChanged()) return true;
        }
        foreach(var loc in myLocations)
        {
            if (loc.Value.isChanged()) return true;
        }
        foreach(var se in mySelectionEvents)
        {
            if (se.Value.isChanged()) return true;
        }
        return myOwnChanges;
    }

    public virtual bool saveMission(string name)
    {
        saveMissionInt(name);

        if (getGamesetChanges() == true)
            setChanged();
        else
            setNotChanged();

        return saveDB(myGsd, "gsd") && saveDB(myGsl, "gsl");
    }

    public virtual bool saveRecord(string name)
    {
        saveRecordInt(name);

        if (getGamesetChanges() == true)
            setChanged();
        else
            setNotChanged();

        return saveDB(myGsd, "gsd") && saveDB(myGsl, "gsl");
    }

    public virtual bool saveSelectionEvent(string name)
    {
        saveSelectionEventInt(name);

        if (getGamesetChanges() == true)
            setChanged();
        else
            setNotChanged();

        return saveDB(myGsd, "gsd") && saveDB(myGsl, "gsl");
    }

    public virtual bool saveLocation(string name)
    {
        saveLocationInt(name);

        if (getGamesetChanges() == true)
            setChanged();
        else
            setNotChanged();

        return saveDB(myGsd, "gsd") && saveDB(myGsl, "gsl");
    }


    public virtual bool saveData()
    {
        // self
        UnivarDBHelper.setVariable<uvs, string>(myGsl.GetRoot(), "Title", myTitle);
        UnivarDBHelper.setVariable<uvs, string>(myGsl.GetRoot(), "Description", myDesc);
        UnivarDBHelper.setStringArray(myGsl.GetRoot(), "Voice Files", myVoices, GamesetDefines.gMaxRadioVoices);

        saveRecordInt(null);
        saveLocationInt(null);
        saveMissionInt(null);
        saveSelectionEventInt(null);

        // messages
        tuvc cont = myGsl.GetRoot().createContainer("Game Messages");
        Savings.saveSimple<Message, uvs>(cont, myMessages);
        cont = null;


        setNotChanged();
        myOwnChanges = false;

        return saveDB(myGsd, "gsd") && saveDB(myGsl, "gsl");
    }

    public virtual IRecord getRecord(int num)
    {
        int i = 0;
        foreach (var rec in myRecords)
        {
            if (i==num)
            {
                return rec.Value;
            }
        }
        return null;

    }


    public virtual ISelectionEvent createSelectionEvent(string name)
    {
        setChanged();
        return createSelectionEventLocal(name);
    }

    public virtual ISelectionEvent getSelectionEvent(int num)
    {
        int i = 0;
        foreach (var rec in mySelectionEvents)
        {
            if (i == num)
            {
                return rec.Value;
            }
        }
        return null;
    }


    public virtual IRecord createRecord(string name)
    {
        setChanged();
        return createRecordLocal(name);
    }

    public virtual IMission getMission(int num)
    {
        int i = 0;
        foreach (var rec in myMissions)
        {
            if (i == num)
            {
                return rec.Value;
            }
        }
        return null;
    }

    public virtual IMission createMission(string name)
    {
        setChanged();
        return createMissionLocal(name);
    }

    public virtual ILocation getLocationByName(string name)
    {
        if (!myLocations.TryGetValue(name, out var loc)) return null;
        return loc;
    }

    public virtual ILocation getLocation(int num)
    {
        int i = 0;
        foreach (var rec in myLocations)
        {
            if (i == num)
            {
                return rec.Value;
            }
        }
        return null;
    }
    public virtual ILocation createLocation(string name)
    {
        setChanged();
        return createLocationLocal(name);
    }

    public virtual string getDescription()
    {
        return myDesc;
    }

    public virtual void setDescription(string desc)
    {
        if (myDesc!= desc)
        {
            myDesc = desc;
            setChanged();
            myOwnChanges = true;
        }
    }

    public virtual string getTitle()
    {
        return myTitle;
    }

    public virtual void setTitle(string desc)
    {
        if (myTitle == desc)
        {
            myTitle = desc;
            setChanged();
            myOwnChanges = true;
        }
    }

    public virtual string getVoice(int i)
    {
        return myVoices[i];
    }
    public virtual void setVoice(int i, string name)
    {
        if ((myVoices[i] == name))
        {
            myVoices[i] = name;
            setChanged();
            myOwnChanges = true;
        }
    }

    // IGamesetMember
    public virtual string getName()
    {
        return myExt.getName();
    }

    public virtual bool setName(string name)
    {
        setChanged();
        myExt.setName(name);
        return true;
    }

    public virtual void deleteMe()
    {
    }

    // IMessages
    public virtual IMessage getMessage(int num)
    {
        int i = 0;
        foreach (var rec in myMessages)
        {
            if (i == num)
            {
                return rec.Value;
            }
        }
        return null;
 
    }

    public virtual IMessage createMessage(string name)
    {
        setChanged();
        return createMessageLocal(name);
    }

    // IGamesetChange
    public virtual bool onRenameRecord(string name)
    {
        return !myRecords.ContainsKey(name) && !myMissions.ContainsKey(name) && !mySelectionEvents.ContainsKey(name);
    }
    public virtual bool onRenameSelectionEvent(string name)
    {
        return !myRecords.ContainsKey(name) && !myMissions.ContainsKey(name) && !mySelectionEvents.ContainsKey(name);
    }

    public virtual void onDeleteRecord(IRecord rec)
    {
        Record tmprec = (Record)rec.Query(Record.ID);
        myRecords.Remove(tmprec.getName());
        setChanged();
    }

    public virtual void onDeleteSelectionEvent(ISelectionEvent rec)
    {
        var tmprec = (SelectionEvent)rec.Query(SelectionEvent.ID);
        mySelectionEvents.Remove(tmprec.getName());
        setChanged();
    }

    public virtual bool onRenameMission(string name)
    {
        return !myRecords.ContainsKey(name) && !myMissions.ContainsKey(name) && !mySelectionEvents.ContainsKey(name);
    }

    public virtual void onChange()
    {
        setChanged();
    }
    public virtual void onChangeMessage()
    {
        setChanged();
    }

    public virtual void onDeleteMission(IMission rec)
    {
        var tmprec = (Mission)rec.Query(Mission.ID);
        myMissions.Remove(tmprec.getName());
        setChanged();
    }

    public virtual bool onRenameLocation(string name)
    {
        return !myLocations.ContainsKey(name);
    }

    public virtual void onDeleteLocation(ILocation rec)
    {
        var tmprec = (Location<RoadBuilderT, RoadNetDataT>)rec.Query(Location<RoadBuilderT,RoadNetDataT>.ID);
        myLocations.Remove(tmprec.getName());
        setChanged();
    }

    // IGamesetChange
    public virtual bool onRenameMessage(string name)
    {
        return !myMessages.ContainsKey(name);
    }

    public virtual void onDeleteMessage(IMessage rec)
    {
        var tmprec = (Message)rec.Query(Message.ID);
        myMessages.Remove(tmprec.getName());
        setChanged();
    }

    public virtual uvc getOnlyMission()
    {
        Asserts.Assert(myExclusiveLocationName != null);
        //return SafeAddRef(myOnlyMission.Ptr());
        return myOnlyMission;
    }

    public virtual uvc getOnlyLocation()
    {
        Asserts.Assert(myExclusiveLocationName != null);
        //return SafeAddRef(myOnlyLocation.Ptr());
        return myOnlyLocation;
    }

    public virtual bool isIntegrityFailed()
    {
        return myData.isIntegrityFailure();
    }

    bool copyMissionInDb(UnivarDB db, string name1, string name2)
    {
        tuvc cont1 = db.GetRoot().openContainer("Events");
        UniCopier un = new UniCopier(cont1, cont1);
        return un.copy(name1, name2);
    }

    bool loadMission(IMission msn)
    {
        string name = msn.getName();
        tuvc gsd = UnivarDBHelper.getMissionContainer(myGsd, name);
        tuvc gsl = UnivarDBHelper.getMissionContainer(myGsl, name);
        Mission smsn = (Mission)msn.Query(Mission.ID);
        return smsn!=null && gsd!=null && gsl!=null && smsn.load(gsd, gsl);
    }

    public virtual IMission copyMission(string name1, string name2)
    {
        //IMission from = myMissions.getByNameWithAddRef(name1);
        //if (from == null) return null;
        //IMission to = createMission(name2);
        //if (to == null) return null;
        if (!myMissions.TryGetValue(name1, out Mission from)) return null;
        if (!myMissions.TryGetValue(name1, out Mission to)) return null;

        if (!saveMission(name1))
            return null;

        return copyMissionInDb(myGsd, name1, name2) && copyMissionInDb(myGsl, name1, name2) && loadMission(to) ? to : null;
    }

    // end of API
    // ==================================================

    bool openDB(UnivarDB db, string ext)
    {
        bool ret = false;
        myExt.setExtension(ext);
        bool create = (myExt.getName() == null);
        if (!create)
            create = db.open(myExt.getFullName(), false) == false;

        Debug.Log(string.Format("Loaded: {0} [{1}]", myExt.getFullName(), create ? "FAIL":"OK"));
        if (create)
            ret = db.create();
        return ret;
    }

    bool saveDB(UnivarDB db, string ext)
    {
        myExt.setExtension(ext);
        return db.save(myExt.getFullName());
    }

    public Gameset(IStormData rs)
    {
        //myData = addRef(rs);
        myData = rs;
        setNotChanged();
        myOwnChanges = false;
        myOnlyLocation = null;
        myOnlyMission = null;
    }

    public bool initialize(string name, ILoadErrorLog log, string msn_name = null)
    {
        myExt = new ExtensionHelper(name);
        bool ret = false;
        bool new_gsd = openDB(myGsd, "gsd");
        if (myGsd.GetRoot() != null)
        {
            openDB(myGsl, "gsl");
            if (myGsl.GetRoot() != null)
            {
                ret = new_gsd ? true : loadData(log, msn_name);
            }
        }
        return ret;
    }

    public bool load<UniType>(UniType gsl, ILoadErrorLog log)
    {
        throw new NotImplementedException();
    }

    public void AddRef()
    {
        throw new NotImplementedException();
    }

    public int RefCount()
    {
        throw new NotImplementedException();
    }

    public int Release()
    {
        throw new NotImplementedException();
    }

    public bool renameNotify(string s)
    {
        throw new NotImplementedException();
    }

    public void deleteNotify(IGamesetMember gsm)
    {
        throw new NotImplementedException();
    }

    public void changeNotify()
    {
        throw new NotImplementedException();
    }

    ~Gameset()
    {
        myExt = null; ;
    }

    UnivarDB myGsd = new UnivarDB();
    UnivarDB myGsl = new UnivarDB();
    bool myOwnChanges;

    ExtensionHelper myExt;
    string myTitle, myDesc;
    string[] myVoices = new string[GamesetDefines.gMaxRadioVoices];

    string myExclusiveLocationName;

    IStormData myData;
    iUnifiedVariableContainer myOnlyMission;
    iUnifiedVariableContainer myOnlyLocation;

    //Dictionary<Record, IGamesetChange> myRecords;
    //Dictionary<SelectionEvent, IGamesetChange> mySelectionEvents;
    //Dictionary<Message, IMessageChange> myMessages;
    //Dictionary<GSLocation, IGamesetChange> myLocations;
    //Dictionary<Mission, IGamesetChange> myMissions;
    Dictionary<string, Record> myRecords = new Dictionary<string, Record>();
    Dictionary<string, SelectionEvent> mySelectionEvents = new Dictionary<string, SelectionEvent>();
    Dictionary<string, Message> myMessages = new Dictionary<string, Message>();
    Dictionary<string, Location<RoadBuilderT, RoadNetDataT>> myLocations = new Dictionary<string, Location<RoadBuilderT, RoadNetDataT>>();
    Dictionary<string, Mission> myMissions = new Dictionary<string, Mission>();
}

public class UniCopier
{
    public UniCopier(iUnifiedVariableContainer src, iUnifiedVariableContainer dest)
    {
        //src->AddRef();
        //dest->AddRef();
        mySrc = src;
        myDest = dest;
    }
    iUnifiedVariableContainer mySrc,myDest;

    public bool copy(string name, string new_name=null)
    {
            return copyElement(mySrc, myDest, name, new_name!=null ? new_name : name);
    }

    private bool copyElement(uvc src, uvc dst, string name, string new_name)
    {
        iUnifiedVariable var = src.GetVariableByName(name);
        if (var == null) return false;

        switch (var.GetClassId())
        {
            case iUnifiedVariableInt.ID:
                copyInt(src, dst, var, name, new_name);
                break;
            case iUnifiedVariableContainer.ID:
                copyContainer(src, dst, var, name, new_name);
                break;
            case iUnifiedVariableArray.ID:
                copyContainerArray(src, dst, var, name, new_name);
                break;
            case iUnifiedVariableFloat.ID:
                copyFloat(src, dst, var, name, new_name);
                break;
            case iUnifiedVariableString.ID:
                copyString(src, dst, var, name, new_name);
                break;
            default:
                Debug.LogError("Failed to load " + name +" " +  var.GetType());
                break;
        }
        return true;
    }

    //Возможно, copy* стоит объединить в один generic метод
    void copyString(iUnifiedVariableContainer src, iUnifiedVariableContainer dst, iUnifiedVariable var, string name, string new_name)
    {
        iUnifiedVariableString value = src.GetVariableTpl<iUnifiedVariableString>(name);
        //Debug.Log("CopyString [" + value + "]");
        if (value == null) return;
        //Debug.Log(string.Format("Creating string {0} \"{1}\" in {2}",new_name,value.GetValue(),dst));
        iUnifiedVariableString lvar2 = dst.CreateVariableTpl<iUnifiedVariableString>(new_name);
        if (lvar2 != null)
        {
           // Debug.Log("Setting value for lvar2");
            lvar2.SetValue(value.GetValue());
        } else
        {
           // Debug.Log("lvar2 creation failed");
        }
        //Debug.Log("CreateVariableTpl [" + lvar2 + "] should be " + value.GetValue());
    }

    void copyFloat(iUnifiedVariableContainer src, iUnifiedVariableContainer dst, iUnifiedVariable var, string name, string new_name)
    {
        //iUnifiedVariableFloat value;
        //if (UnivarDBHelper.getVariable<UniType, CType>(src, name, value))
        //{
        //    TRef<UniType> lvar2 = dst->CreateVariableTpl<UniType>(new_name);
        //    if (lvar2)
        //        lvar2->SetValue(value);
        //}

        iUnifiedVariableFloat value = src.GetVariableTpl<iUnifiedVariableFloat>(name);
        if (value == null) return;
        Debug.Log("Copying value: " + value.GetValue() + " from " + name);
        iUnifiedVariableFloat lvar2 = dst.CreateVariableTpl<iUnifiedVariableFloat>(new_name);
        if (lvar2 != null) lvar2.SetValue(value.GetValue());
        Debug.Log("Set new value: " + lvar2.GetValue() + " to " + new_name);

    }

    void copyInt(iUnifiedVariableContainer src, iUnifiedVariableContainer dst, iUnifiedVariable var, string name, string new_name)
    {
        iUnifiedVariableInt value = src.GetVariableTpl<iUnifiedVariableInt>(name);
        if (value == null) return;
        iUnifiedVariableInt lvar2 = dst.CreateVariableTpl<iUnifiedVariableInt>(new_name);
        if (lvar2 != null) lvar2.SetValue(value.GetValue());
    }

    //void copyContainer<UniType>(iUnifiedVariableContainer src, iUnifiedVariableContainer dst, iUnifiedVariable var, string name, string new_name) where UniType: iUnifiedVariableContainer
    //{
    //    Debug.Log(src);
    //    UniType cont = var.Query<UniType>();
    //    Debug.Log(cont);
    //    copyContainerRecurse<UniType>(cont, dst, name, new_name);
    //}

    void copyContainer(iUnifiedVariableContainer src, iUnifiedVariableContainer dst, iUnifiedVariable var, string name, string new_name) 
    {
        iUnifiedVariableContainer cont = (iUnifiedVariableContainer)var.Query(iUnifiedVariableContainer.ID);
        copyContainerRecurse(cont, dst, name, new_name);
    }

    void copyContainerArray(iUnifiedVariableContainer src, iUnifiedVariableContainer dst, iUnifiedVariable var, string name, string new_name)
    {
        iUnifiedVariableArray cont = (iUnifiedVariableArray)var.Query(iUnifiedVariableArray.ID);
        //Debug.Log(string.Format("{0} -> {1}\nfrom {2}\nto {3} using {4}",name,new_name,src,dst,var));
        copyContainerArrayRecurse(cont, dst, name, new_name);
    }
    void copyContainerArrayRecurse(iUnifiedVariableContainer src, iUnifiedVariableContainer dst, string name, string new_name)
    {
        // create contariner
        iUnifiedVariableArray dest = dst.CreateVariableTpl<iUnifiedVariableArray>(new_name);
        if (dest != null)
        {
            int handle = (int)src.GetNextHandle(0);
            
            while (handle != 0)
            {
                //Debug.Log("copyContainerArrayRecurse using handle " + handle.ToString("X8"));
                copyElement(src, (iUnifiedVariableArray)dest, handle);
                handle = (int)src.GetNextHandle((uint)handle);
            }
        }
    }

    void copyContainerRecurse(iUnifiedVariableContainer src, iUnifiedVariableContainer dst, string name, string new_name)
    {
        // create contariner
        iUnifiedVariableContainer dest = dst.CreateVariableTpl<iUnifiedVariableContainer>(new_name);
        if (dest != null)
        {
            int handle = (int)src.GetNextHandle(0);
            while (handle != 0)
            {
                copyElement(src, (iUnifiedVariableContainer)dest, handle);
                handle = (int)src.GetNextHandle((uint)handle);
            }
        }
    }

    void copyContainerRecurse<UniType>(iUnifiedVariableContainer src, iUnifiedVariableContainer dst, string name, string new_name) where UniType:iUnifiedVariable
    {
        // create contariner
        UniType dest = dst.CreateVariableTpl<UniType>(new_name);
        if (dest!=null)
        {
            int handle = (int) src.GetNextHandle(0);
            while (handle!=0)
            {
                copyElement(src, (iUnifiedVariableContainer) dest, handle);
                handle = (int)src.GetNextHandle((uint)handle);
            }
        }
    }

    bool copyElement(iUnifiedVariableContainer src, iUnifiedVariableContainer dst, int handle)
    {
        //char buffer[256];
        //StrCpy(buffer, src->GetNameByHandle(buffer, handle));
        string buffer="";
        src.GetNameByHandle(ref buffer, (uint)handle);
        //Debug.Log("GetNameByHandle from " + handle.ToString("X8") + " to " + buffer);
        //Debug.Log("src name [" + buffer + "] " + src);
        //Debug.Log("dst name [" + buffer + "] " + dst);
        return copyElement(src, dst, buffer, buffer);
    }
}

public static class GamesetDefines
{
    public const string gsEmpty = "";
    public const int gMaxRadioVoices = 11;
}