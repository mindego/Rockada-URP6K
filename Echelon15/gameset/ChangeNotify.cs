using System.Collections;
using UnityEngine;
using crc32 = System.UInt32;

public enum ChangeStatus
{
    csNotChanged,
    csChanged,
    csDeleted
}

public interface IStatus
{
    public const uint ID = 0x7E6ED1A2;
    public ChangeStatus getStatus();
};


public class Status : IStatus
{
    public virtual ChangeStatus getStatus()
    {
        return myStatus;
    }
    public IStatus getIStatus() { return this; }

    public Status()
    {
        myStatus = ChangeStatus.csChanged;
    }

    public void setNotChanged()
    {
        myStatus = ChangeStatus.csNotChanged;
    }
    public void setChanged()
    {
        myStatus = ChangeStatus.csChanged;
    }
    public void notChanged()
    {
        myStatus = ChangeStatus.csNotChanged;
    }
    public void setDeleted()
    {
        myStatus = ChangeStatus.csDeleted;
    }
    protected ChangeStatus myStatus;
};

public interface IGamesetChange : IGamesetChanger<IGamesetChange>
{
    public bool onRenameRecord(string name);
    public void onDeleteRecord(IRecord r);

    public bool onRenameSelectionEvent(string name);
    public void onDeleteSelectionEvent(ISelectionEvent se);

    public bool onRenameMission(string name);
    public void onDeleteMission(IMission m);

    public bool onRenameLocation(string name);
    public void onDeleteLocation(ILocation l);
    public void onChange();
};

/// <summary>
/// Класс-заглушка для использования в generic классе GameSet
/// </summary>
public interface IGamesetChanger<Changer>
{
    //public delegate bool myrenameNotify(string s);
    //public delegate void mydeleteNotify(IGamesetMember gsm);
    //public delegate void mychangeNotify();
    public bool renameNotify(string s);
    public void deleteNotify(IGamesetMember gsm);
    public void changeNotify();
}
public interface IMessageChange : IGamesetChanger<IMessageChange>
{
    public bool onRenameMessage(string name);
    public void onChangeMessage();
    public void onDeleteMessage(IMessage m);
};

public interface IGroupChange: IGamesetChanger<IGroupChange>
{
    public void onDeleteUnit(IUnit u);
    public void onChangeUnit();
    public bool onRenameUnit(string s) { return true; }
    public void onDeletePoint(IRoutePoint p);
    public void onChangePoint();
};

public interface IStormData : IObject
{
    public string resolveObjectName(crc32 name);
    public string resolveWeaponName(crc32 name);
    public string resolveRoadName(crc32 name);
    public UnitAttribute resolveUnitAttr(crc32 name);
    public void processIntegrityFailure();
    public bool isIntegrityFailure();
    public ROADDATA getRoadByCode(crc32 code, bool mustExist);
};

public interface IGroupHolderChange : IGamesetChanger<IGroupHolderChange>
{
    public bool onRenameGroup(string name);
    public void onDeleteGroup(IGroup g);
    public void onChangeGroup();

    public bool onRenameMarker(string name);
    public void onChangeMarker();
    public void onDeleteMarker(IMarker m);
};



public interface IPointHolderChange
{
    public void onDeletePoint(IPoint p);
    public void onChangePoint();
};

public interface IRoadHolderChange : IGamesetChanger<IRoadHolderChange>
{
    public bool onRenameRoad(string name) { return true; }
    public void onDeleteRoad(IRoad r);
    public void onChangeRoad();
};

