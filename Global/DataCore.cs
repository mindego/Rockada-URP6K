using System;
using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;
using uvc = iUnifiedVariableContainer;

public class TimeInfo
{
    public int IsOffset;
    public int wYear;
    public int wMonth;
    public int wDay;
    public int wHour;
    public int wMinute;

    //bool operator ==(const TimeInfo & p2) {
    //    return
    //    p2.IsOffset == IsOffset &&
    //    p2.wYear    == wYear &&
    //    p2.wMonth   == wMonth &&
    //    p2.wDay     == wDay &&
    //    p2.wHour    == wHour &&
    //    p2.wMinute  == wMinute;
    //}

    public void set(int off, int year, int m, int d, int h, int minute)
    {
        IsOffset = off;
        wYear = year;
        wMonth = m;
        wDay = d;
        wHour = h;
        wMinute = minute;
    }

    public override bool Equals(object obj)
    {
        return obj is TimeInfo info &&
               IsOffset == info.IsOffset &&
               wYear == info.wYear &&
               wMonth == info.wMonth &&
               wDay == info.wDay &&
               wHour == info.wHour &&
               wMinute == info.wMinute;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsOffset, wYear, wMonth, wDay, wHour, wMinute);
    }

    public TimeInfo()
    {
        IsOffset = 1;
        wYear = 0;
        wMonth = 0;
        wDay = 0;
        wHour = 0;
        wMinute = 0;
    }
}
public interface IEvent : IGamesetMember
{
    public bool isChanged();
    public string getTitle();
    public void setTitle(string desc);

    public string getDescription();
    public void setDescription(string desc);

    public void setTime(TimeInfo tm);
    public TimeInfo getTime();
};

public interface IScriptableEvent : IEvent
{
    public void setNextScript(string script);
    public string getNextScript();
};

public interface IUnit : IGamesetMember
{
    public string getAi();
    public void setAi(string text);

    public string getAiScript();
    public void setAiScript(string text);

    public float getAngle();
    public void setAngle(float angle);

    public int getFlags();
    public void setFlags(int flags);

    public uint getLayout(int i);
    public void setLayout(int i, uint name);

    public Vector3 getPos();
    public void setPos(Vector3 v);

    public UnitType getAttr();
};

public interface IPoint : IDeletableMember
{
    public Vector3 getPos();
    public void setPos(Vector3 v);
};


public interface IRoutePoint : IPoint
{
    public void setAiScript(string script);
    public string getAiScript();
};

public interface IRoutes
{
    public IRoutePoint createPoint();
    public IRoutePoint insertPoint(int pos);
    public IRoutePoint getRoutePoint(int pos);
};

public interface IGroup : IGamesetMember, IRoutes
{
    public string getAi();
    public void setAi(string text);

    public string getAiScript();
    public void setAiScript(string text);

    public int getSide();
    public void setSide(int side);

    public int getVoice();
    public void setVoice(int voice);

    public int getFlags();
    public void setFlags(int flags);

    public IUnit getUnit(int i);
    public IUnit getPoint(int i) { return getUnit(i); }

    // unified
    public IUnit getItem(int i) { return getUnit(i); }
    public IUnit createUnit();
    public IUnit insertUnit(int pos);
};

public interface IGroups
{
    public IGroup getGroup(int i);
    public IGroup getGroupByName(string name);
    // unified
    public IGroup getObject(int i)
    {
        return getGroup(i);
    }
    public IGroup createGroup(string name);
    // unified
    public IGroup createObject(string name)
    {
        return createGroup(name);
    }
};

public interface IMarker : IGamesetMember
{
    public Vector3 getPos();
    public void setPos(Vector3 v);

    public float getRadius() { return getPos().y; }
    public void setRadius(float rad);
    //{
    //    Vector3pos = getPos();
    //    setPos(VECTOR(pos.x, rad, pos.z));
    //}
    public Vector2 getPosition();
    //    {
    //        Vector3pos = getPos();
    //        return Vector2f(pos.x, pos.z);
    //}
    void setPosition(Vector2 pos);
    //{
    //    setPos(VECTOR(pos.x, getRadius(), pos.y));
    //}

}

public interface IMarkers
{
    public IMarker getMarker(int i);
    public IMarker getMarkerByName(string name);
    public IMarker createMarker(string name);
};

public interface IAiScriptOwner
{
    public string getAi();
    public void setAi(string text);

    public string getAiScript();
    public void setAiScript(string text);
};

public interface IMission : IGamesetMember, IScriptableEvent, IMessages, IGroups, IMarkers, IAiScriptOwner
{
    public string getAiDlls();
    public string getVisConfigName();
    public string getLocation();
    public string getBriefing();
    public string getDebrOnSuccess();
    public string getDebrOnFailure();
    public bool isWeaponEnabled(crc32 name);
    public bool isCraftEnabled(crc32 name);

    public void setAiDlls(string text);
    public void setVisConfigName(string text);
    public void setLocation(string loc_name);
    public void setBriefing(string text);
    public void setDebrOnSuccess(string text);
    public void setDebrOnFailure(string text);
    public void enableWeapon(crc32 name, bool enable);
    public void enableCraft(crc32 name, bool enable);
};

public interface IGameset : IGamesetMember, IMessages
{
    public const uint GAMESET_VERSION = 0x5063A56D;
    public uvc getOnlyMission();
    public uvc getOnlyLocation();
    public IMission copyMission(string name1, string name2);

    public bool saveData();

    public bool saveMission(string name);
    public bool saveRecord(string name);
    public bool saveLocation(string name);
    public bool saveSelectionEvent(string name);

    public ISelectionEvent createSelectionEvent(string name);
    public ISelectionEvent getSelectionEvent(int num);

    public IRecord createRecord(string name);
    public IRecord getRecord(int num);

    public IMission createMission(string name);
    public IMission getMission(int num);

    public ILocation getLocation(int num);
    public ILocation getLocationByName(string name);
    public ILocation createLocation(string name);

    public string getDescription();
    public string getTitle();
    public string getVoice(int i);

    public void setDescription(string desc);
    public void setTitle(string desc);
    public void setVoice(int i, string name);

    public bool isChanged();
    public bool isIntegrityFailed();
};

//interface IHash : IRefMem, IHAccess


public interface IDeletableMember : IObject
{
    public void deleteMe();
};

public interface ILoadableMember : IObject
{
    //public bool load<UniType>(UniType type, ILoadErrorLog log);
    public bool load<UniType>(UniType type, ILoadErrorLog log);
}

public interface ILoadableTransMember : IObject
{
    //public bool load<UniType>(UniType gsd, UniType gsl, ILoadErrorLog log);
    public bool load(iUnifiedVariableContainer gsd, iUnifiedVariableContainer gsl, ILoadErrorLog log);
}


//public interface IGamesetMember<Parent, Changer> : IDeletableMember
public interface IGamesetMember : IDeletableMember
{
    public string getName();
    public bool setName(string name);
};

public interface IMessages
{
    public IMessage getMessage(int i);
    public IMessage createMessage(string name);
};

public interface IMessage : IGamesetMember
{
    public string getText();
    public void setText(string name);
};




/// <summary>
/// General sound api
/// </summary>
public interface ISound : IRefMem
{
    /// <summary>
    /// 2d sound
    /// </summary>
    /// <param name="scale"></param>
    public void Update(float scale);
    public void SetMasterVolume(int i);
    public int GetMasterVolume();

    public I3DSound Create3D(int volume = 9, bool can_edit = true);
    public IStreamSoundData OpenWaveFile(string name, bool loop);
    public IStreamAudio CreateStreamAudio(IStreamSoundData ssd, int nSeconds);
    public IVoice CreateVoice(bool fake = false);

    public float convertVolume(int vol);
};

/// <summary>
/// Voice creation interface
/// </summary>
public interface IVoice : IRefMem
{
    public IObject OpenVoiceDB(string db_name);

    /// <summary>
    /// load from disk
    /// </summary>
    /// <param name="db"></param>
    /// <param name=""></param>
    /// <returns></returns>
    public bool CachePhrase(IObject db, string s); // load from disk
    public IWave CreateWave(IObject db, string s); // one buffer
    public IWave CreatePhrase(IObject db, string s); // buffer queue
};

/// <summary>
/// Straming (sic!) audio interface
/// </summary>
public interface IStreamAudio : IRefMem
{
    public bool Update();

    public bool Play();
    public void Stop();

    public void SetVolume(int vol);
    public int GetVolume();
};

/// <summary>
/// Wave/Phrase interface
/// </summary>
public interface IWave : IRefMem
{
    /// <summary>
    /// ignored for Phrase (loop parameter)
    /// </summary>
    /// <param name="looped">ignored for Phrase</param>
    /// <param name="vol"></param>
    public void Play(bool looped, int vol);
    public void Stop();
    public void SetFrequecy(float frq);
    /// <summary>
    /// NOT IMPLEMENTER for Phrase
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying();
};

public struct SSDInfo
{
    public int Length;
};

public interface IStreamSoundData : IObject
{
    public void GetStreamInfo(ref SSDInfo info);
    public void GetDataFormat(ref PCMWAVEFORMAT data );
    /// <summary>
    /// returned number of samples succeeded operation
    /// </summary>
    /// <param name="num_samples"></param>
    /// <returns>number of samples succeeded operation</returns>
    public int SkipData(int num_samples);
    public int GetData(int num_samples, object pdata, int cbytes);
};

public struct SVDInfo
{
    public int Length;
    public float FramesPerSecond;
};

public interface IStreamVideoData : IObject
{
    public void GetStreamInfo(SVDInfo info);
    //public void GetDataFormat(BITMAPINFOHEADER header );
    public int Skip(int n_frames);//actual frames skipped
    public int GetFrame(object pdata);//if pdata==0 then needed size returned
};

public interface IMenuFeedback : IObject
{
    public bool showEngBay(EngBayParams ebp);
    public void onServerShutdown();
    public void onStartDraw();
};
public interface IRoad : IGamesetMember
{
    public IPoint getPoint(int i);
    public IPoint getItem(int i) { return getPoint(i); }
    public IPoint createPoint();
    public IPoint insertPoint(int pos);
};

public interface IRoads
{
    public IRoad createRoad(string name);
    // unified
    public IRoad createObject(string name)
    {
        return createRoad(name);
    }
    public IRoad getRoad(int i);
    // unified
    IRoad getObject(int i)
    {
        return getRoad(i);
    }
};

public interface ILocation : IGamesetMember, IGroups, IMarkers, IRoads
{
    public crc32 getID();
    public string getAiDlls();
    public int getGameMapSizeX();
    public int getGameMapSizeZ();
    public string getGameMapName();
    public string getMeMapName();
    public string getTerrainName();
    public IDataBlock getRoadNet();

    public bool isChanged();
    public void setAiDlls(string dlls);
    public void setGameMapName(string name);
    public void setGameMapSizeX(int x);
    public void setGameMapSizeZ(int z);
    public void setMeMapName(string name);
    public void setTerrainName(string name);
};

public class StormTimer : ITimer
{
    double timer;

    public StormTimer(double timer = 0)
    {
        this.timer = timer;
    }

    public static ITimer CreateTimer()
    {
        return new StormTimer();
    }
    public void AddRef()
    {
        throw new NotImplementedException();
    }

    public double GetAvrgFPS()
    {
        throw new NotImplementedException();
    }

    public double GetTime()
    {
        return timer;
    }

    public int RefCount()
    {
        throw new NotImplementedException();
    }

    public int Release()
    {
        throw new NotImplementedException();
    }

    public double Update()
    {
        timer += Time.deltaTime;
        return timer; ; //TODO передавать что нужно
    }
}



