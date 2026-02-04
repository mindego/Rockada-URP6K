using BaseMission = ScriptableEvent<IMission, IGamesetChange>;
using crc32 = System.UInt32;
using uvc = iUnifiedVariableContainer;
using uvs = iUnifiedVariableString;
using uvv = iUnifiedVariableVector;
using tuvc = iUnifiedVariableContainer;
using System.Collections.Generic;
using UnityEngine;
//typedef ScriptableEvent<IMission, IGamesetChange, IGamesetChange::onRenameMission, IGamesetChange::onDeleteMission, IGamesetChange::onChange> BaseMission;
//interface Mission : BaseMission, IMessageChange, IGroupHolderChange

public class Mission : BaseMission, IMessageChange, IGroupHolderChange, IMission, ILoadableTransMember
{
    public const uint ID = 0xA025345F;
    // IObject
    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IStatus.ID: return getIStatus();
            case Mission.ID: return this;
            default: return 0;
        }
    }

    private int find(Tab<crc32>  vct, crc32 name) {
            for (int i=0;i<vct.Count();i++)
                if (vct[i]==name) return i;
            return -1;
        }
    void insert_unique(ref Tab<crc32> vct, crc32 name)
    {
        int idx = find(vct, name);
        if (idx == -1)
            vct.New(name);
    }
    void erase(ref Tab<crc32> vct, crc32 name)
    {
        int idx = find(vct, name);
        if (idx != -1)
            vct.Remove(idx);
    }

    public override bool setName(string name)
    {
        bool ret = base.setName(name);
        if (ret)
        foreach (var rec in myGroups)
            {
                rec.Value.setChanged();
            }
        return ret;
    }
    // IMisssion
    public virtual bool isWeaponEnabled(crc32 name)
    {
        return find(myEnabledWeapons, name) != -1;
    }
    public virtual bool isCraftEnabled(crc32 name)
    {
        return find(myEnabledCrafts, name) != -1;
    }
    public virtual void enableWeapon(crc32 name, bool enable)
    {
        if (enable)
            insert_unique(ref myEnabledWeapons, name);
        else
            erase(ref myEnabledWeapons, name);
        setChanged();
        myChange.onChange();
    }
    public virtual void enableCraft(crc32 name, bool enable)
    {
        if (enable)
            insert_unique(ref myEnabledCrafts, name);
        else
            erase(ref myEnabledCrafts, name);
        setChanged();
        myChange.onChange();
    }
    public virtual string getAiDlls() { return myAiDlls; }
    public virtual void setAiDlls(string text)
    {
        if (myAiDlls == text) return;
        myAiDlls = text;
        myChange.onChange();
        setChanged();
    }

    public virtual string getAi() { return myAi; }
    public virtual void setAi(string text)
    {
        if (myAi == text) return;
        myAi = text;
        myChange.onChange();
        setChanged();
    }

    public virtual string getAiScript() { return myAiScript; }
    public virtual void setAiScript(string text)
    {
        if (myAiScript == text) return;
        myAiScript = text;
        myChange.onChange();
        setChanged();
    }

    public virtual string getVisConfigName() { return myVisConfigName; }
    public virtual void setVisConfigName(string text)
    {
        if (myVisConfigName ==text) return;
        myVisConfigName = text;
        myChange.onChange();
        setChanged();
    }

    public virtual string getBriefing() { return myBriefing; }
    public virtual void setBriefing(string text)
    {
        if (myBriefing == text) return;
        myBriefing = text;
        myChange.onChange();
        setChanged();
    }

    public virtual string getDebrOnSuccess() { return myDebrOnSuccess; }
    public virtual void setDebrOnSuccess(string text)
    {
        if (myDebrOnSuccess == text) return;
        myDebrOnSuccess = text;
        myChange.onChange();
        setChanged();
    }

    public virtual string getDebrOnFailure() { return myDebrOnFailure; }
    public virtual void setDebrOnFailure(string text)
    {
        if (myDebrOnFailure == text) return;
        myDebrOnFailure = text;
        myChange.onChange();
        setChanged();
    }

    public virtual string getLocation()
    {
        return myLocation;
    }
    public virtual void setLocation(string name)
    {
        if (myLocation == name) return;
        myLocation = name;
        myChange.onChange();
        setChanged();
    }
    // IMessages
    public virtual IMessage getMessage(int i)
    {
        int j = 0;
        foreach(var rec in myMessages)
        {
            if (j++ == i) return rec.Value;
        }
        //IMessage msg = myMessages.get(i);
        //return SafeAddRef(msg);
        //return IRefMem.SafeAddRef(msg);
        return null;
    }

    public virtual IMessage createMessage(string name)
    {
        myChange.onChange();
        setChanged();
        return (IMessage) createMessageLocal(name);
    }

    // IGroups
    public virtual IGroup getGroup(int i)
    {
        int j = 0;
        foreach (var rec in myGroups)
        {
            if (j++ == i) return rec.Value;
        }
        //IGroup rec = myGroups.get(i);
        //return IRefMem.SafeAddRef(rec);
        return null;
    }
    public virtual IGroup getGroupByName(string name)
    {
        crc32 hash_name = Hasher.HshString(name);
        //IGroup rec;
        foreach (var rec in myGroups)
        {
            if (Hasher.HshString(rec.Value.getName()) == hash_name) return rec.Value;
        }

        return null;
    }
    public virtual IGroup createGroup(string name)
    {
        myChange.onChange();
        setChanged();
        return createGroupLocal(name);
    }

    // IMarkers
    public virtual IMarker getMarker(int i)
    {
        int j = 0;
        foreach (var rec in myMarkers)
        {
            if (j++ == i) return rec.Value;
        }
        return null;
        //IMarker rec = myMarkers.get(i);
        //return IRefMem.SafeAddRef(rec);
    }
    // IMarkers
    public virtual IMarker getMarkerByName(string name)
    {
        crc32 hash_name = Hasher.HshString(name);
        //IMarker rec;
        //for (int i = 0; (rec = myMarkers.get(i)) !=null; i++)
        //{
        //    if (Hasher.HshString(rec.getName()) == hash_name)
        //        return IRefMem.SafeAddRef(rec);
        //}

        foreach (var rec in myMarkers)
        {
            if (Hasher.HshString(rec.Value.getName()) == hash_name) return rec.Value;
        }
        return null;
    }
    public virtual IMarker createMarker(string name)
    {
        myChange.onChange();
        return createMarkerLocal(name);
    }

    // IMessageChange
    public virtual bool onRenameMessage(string name)
    {
        return !myMessages.ContainsKey(name);
    }

    public virtual void onDeleteMessage(IMessage msg)
    {
        setChanged();
        var rec = (Message)msg.Query(Message.ID);
        myMessages.Remove(rec.getName());
        myChange.onChange();
    }

    // IGroupHolderChange
    public virtual bool onRenameGroup(string name)
    {
        return !myGroups.ContainsKey(name);
    }

    public virtual void onChangeGroup()
    {
        setChanged();
        myChange.onChange();
    }
    public virtual void onChangeMessage()
    {
        setChanged();
        myChange.onChange();
    }
    public virtual void onChangeMarker()
    {
        setChanged();
        myChange.onChange();
    }

    public virtual void onDeleteGroup(IGroup grp)
    {
        setChanged();
        //myGroups.remove((Group)grp.Query(Group.ID));
        var tmpGroup = (Group)grp.Query(Group.ID);
        myGroups.Remove(tmpGroup.getName());
        myChange.onChange();
    }

    public virtual bool onRenameMarker(string name)
    {
        return !myMarkers.ContainsKey(name);
    }

    public virtual void onDeleteMarker(IMarker mrk)
    {
        setChanged();
        var rec = (Marker)mrk.Query(Marker.ID);
        myMarkers.Remove(rec.getName());
        //myMarkers.remove((Marker)mrk.Query(Marker.ID));
        myChange.onChange();
    }

    // self
    Message createMessageLocal(string name)
    {
        var rec = new Message(name, this);
        myMessages.Add(name, rec);
        return rec;
        //return myMessages.add(name, this);
    }

    Group createGroupLocal(string name)
    {
        //return myGroups.add(name, this, myData);
        var rec = new Group(name, this, myData);
        myGroups.Add(name, rec);
        return rec;
    }

    Marker createMarkerLocal(string name)
    {
        var rec = new Marker(name, this);
        myMarkers.Add(name, rec);
        return rec;
        //return myMarkers.add(name, this);
    }

    public override bool save(uvc gsd, uvc gsl)
    {
        if (!base.save(gsd, gsl)) return false;
        //gsd.setInt("Type", 0xA025345F);
        gsd.setInt("Type", Mission.ID);
        if (!gsl.setString("Briefing", myBriefing)) return false;
        if (!gsl.setString("DebrSuccess", myDebrOnSuccess)) return false;
        if (!gsl.setString("DebrFailure", myDebrOnFailure)) return false;
        if (!gsd.setString("Ai", myAi)) return false;
        if (!gsd.setString("AiDlls", myAiDlls)) return false;
        if (!gsd.setString("AiScript", myAiScript)) return false;
        if (!gsd.setString("VisConfigName", myVisConfigName)) return false;
        if (!gsd.setBlock("EnabledWeapons", myEnabledWeapons)) return false;
        if (!gsd.setBlock("EnabledCrafts", myEnabledCrafts)) return false;

        //char buffer[MAX_PATH];
        //wsprintf(buffer, "\\Root\\Locations\\%s", string(myLocation));
        string buffer = string.Format("\\Root\\Locations\\{0}", myLocation);
        if (!gsd.setRef("Location", buffer)) return false;
        tuvc cont;

        cont = gsl.createContainer("Game Messages");
        Asserts.Assert(cont!=null);
        Savings.saveSimple<Message, uvs>(cont, myMessages);

        cont = gsd.createContainer("Groups");
        Asserts.Assert(cont!=null);
        Savings.saveSimple<Group, uvc>(cont, myGroups);

        cont = gsd.createContainer("Markers");
        Asserts.Assert(cont!=null);
        Savings.saveSimple<Marker, uvv>(cont, myMarkers);

        setNotChanged();

        saveNotify();

        return true;
    }

    public override bool load(uvc gsd, uvc gsl, ILoadErrorLog log = null)
    {
        if (!base.load(gsd, gsl, log)) return false;
        gsl.getString("Briefing", ref myBriefing);
        gsl.getString("DebrSuccess", ref myDebrOnSuccess);
        gsl.getString("DebrFailure", ref myDebrOnFailure);
        if (!Savings.loadStringVar("Ai", ref myAi, gsd, log)) return false;
        if (!Savings.loadStringVar("AiDlls", ref myAiDlls, gsd, log)) return false;
        gsd.getString("AiScript", ref myAiScript);

        if (!Savings.loadStringVar("VisConfigName", ref myVisConfigName, gsd, log)) return false;
        //gsd.getBlock("EnabledWeapons", out myEnabledWeapons,sizeof(crc32));
        //gsd.getBlock("EnabledCrafts", out myEnabledCrafts, sizeof(crc32));
        gsd.GetBlockDWORD("EnabledWeapons", out myEnabledWeapons);
        gsd.GetBlockDWORD("EnabledCrafts", out myEnabledCrafts);
        if (!Savings.loadRefVar("Location", ref myLocation, gsd, log)) return false;
        string discard=null;
        string name = wintools.GetFnDir(myLocation, ref discard);
        if (name!=null)
        {
            //char buffer[MAX_PATH];
            //_addstr(buffer, name);
            //myLocation = buffer;
            myLocation = name; //TODO - проверить корректность задачи пути
        }
        else
            myLocation = GamesetDefines.gsEmpty;
        tuvc cont;
        //Debug.Log("Loading Messages");
        cont = gsl.createContainer("Game Messages");
        Savings.enumSimple<Message, uvs, Mission>(cont, this, createMessageLocal, log);
        //Debug.Log("Loading Markers");
        cont = gsd.createContainer("Markers");
        Asserts.Assert(cont!=null);
        Savings.enumSimple<Marker, uvv, Mission>(cont, this, createMarkerLocal, log);
        //Debug.Log("Loading Groups");
        cont = gsd.createContainer("Groups");
        Asserts.Assert(cont!=null);
        Savings.enumSimple<Group, uvc, Mission>(cont, this, createGroupLocal, log);

        setNotChanged();
        return true;
    }

    //typedef ScriptableEvent<IMission, IGamesetChange, IGamesetChange::onRenameMission, IGamesetChange::onDeleteMission, IGamesetChange::onChange> BaseMission;
    public bool renameNotify(string s)
    {
        return onRenameMessage(s);
    }

    public void changeNotify()
    {
        throw new System.NotImplementedException();
    }

    public void deleteNotify(IGamesetMember gsm)
    {
        throw new System.NotImplementedException();
    }

    public Mission(string name, IGamesetChange ch, IStormData data) : base(name, ch)
    {
        myData = data;
    }

    // IMessageChange

    protected Tab<crc32> myEnabledWeapons, myEnabledCrafts;
    protected string myAiDlls;
    protected string myAi;
    protected string myAiScript;
    protected string myVisConfigName;
    protected string myBriefing;
    protected string myDebrOnSuccess;
    protected string myDebrOnFailure;
    protected string myLocation;

    IStormData myData;

    Dictionary<string,Message> myMessages = new Dictionary<string, Message>();
    Dictionary<string,Group> myGroups = new Dictionary<string, Group>();
    Dictionary<string,Marker> myMarkers = new Dictionary<string, Marker>();
}