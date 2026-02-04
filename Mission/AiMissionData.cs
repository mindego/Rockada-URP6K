using System.Collections.Generic;
using DWORD = System.UInt32;
using crc32 = System.UInt32;
using UnityEngine;
using System.Text;
using static CampaignDefines;
/// <summary>
/// AiMissionData - представление миссии для ИИ
/// </summary>
public class AiMissionData
{

    // API
    // default groups (from proto-mission)
    public DWORD GetDefGroupsCount() { return mDefGroupsCount; }
    public GROUP_DATA[] GetDefGroups() { return mpDefGroups; }
    public GROUP_DATA GetDefGroupByID(DWORD ID)
    {
        return FindHashedData<GROUP_DATA>(mpDefGroups, mDefGroupsCount, ID);
    }
    // mission-specific group
    public DWORD GetGroupsCount() { return mGroupsCount; }
    public GROUP_DATA[] GetGroups() { return mpGroups; }
    public GROUP_DATA GetGroupByID(DWORD ID)
    {
        return FindHashedData<GROUP_DATA>(mpGroups, mGroupsCount, ID);

    }
    // markers
    public DWORD GetMarkersCount() { return mMarkersCount; }
    public MARKER_DATA[] GetMarkers() { return mpMarkers; }
    public MARKER_DATA GetMarkerByID(DWORD ID)
    {
        return FindHashedData<MARKER_DATA>(mpMarkers, mMarkersCount, ID);
    }
    // misc
    public DWORD GetCraftCount() { return (uint)myEnabledCrafts.Count(); }
    public crc32[] GetCrafts() { return myEnabledCrafts.Begin(); }
    public DWORD GetWeaponsCount() { return (uint)myEnabledWeapons.Count(); }
    public crc32[] GetWeapons() { return myEnabledWeapons.Begin(); }
    public string GetAiScript() { return mpAiScript; }
    public string GetAi() { return mpAi; }
    public string GetTitle() { return mpTitle; }


    // own
    private GROUP_DATA[] mpDefGroups;
    private DWORD mDefGroupsCount;
    GROUP_DATA[] mpGroups;
    DWORD mGroupsCount;
    MARKER_DATA[] mpMarkers;
    DWORD mMarkersCount;
    string mpAiScript;
    string mpAi;
    string mpTitle;
    Tab<crc32> myEnabledCrafts;
    Tab<crc32> myEnabledWeapons;
    public AiMissionData()
    {
        mpDefGroups = new GROUP_DATA[0];
        mDefGroupsCount = 0;
        mpGroups = new GROUP_DATA[0];
        mGroupsCount = 0;
        mpMarkers = new MARKER_DATA[0];
        mMarkersCount = 0;
        mpAiScript = string.Empty;
        mpAi = string.Empty;
        mpTitle = string.Empty;
    }

    private bool patch;
    private MissionPatch missionPatch;
    public void Patch(MissionPatch missionPatch)
    {
        Debug.Log("Patching mission");
        patch = true;
        this.missionPatch = missionPatch;
    }

    public void Init(iUnifiedVariableContainer pDefMsn, iUnifiedVariableContainer pMsn)
    {
        DWORD h = 0;
        string spGroupsName = "Groups";
        string spMarkersName = "Markers";
        string spAiScriptName = "AiScript";
        string spAiName = "Ai";
        string spTitleName = "Title";

        // получаем контейнеры групп для DefaultMission и Msn
        UniVarContainer pDefGroups = (UniVarContainer)pDefMsn.GetVariableTpl<iUnifiedVariableContainer>(spGroupsName);
        UniVarContainer pGroups = (UniVarContainer)pMsn.GetVariableTpl<iUnifiedVariableContainer>(spGroupsName);
        UniVarContainer pMarkers = (UniVarContainer)pMsn.GetVariableTpl<iUnifiedVariableContainer>(spMarkersName);

        UniVarString pAiScript = (UniVarString)pMsn.GetVariableTpl<iUnifiedVariableString>(spAiScriptName);
        UniVarString pAi = (UniVarString)pMsn.GetVariableTpl<iUnifiedVariableString>(spAiName);
        UniVarString pTitle = (UniVarString)pMsn.GetVariableTpl<iUnifiedVariableString>(spTitleName);

        // выделяем массивы
        mDefGroupsCount = pDefGroups.GetSize();
        //mpDefGroups = (mDefGroupsCount > 0 ? new GROUP_DATA[mDefGroupsCount] : null);
        mpDefGroups = new GROUP_DATA[mDefGroupsCount];
        mGroupsCount = pGroups.GetSize();
        //mpGroups = (mGroupsCount > 0 ? new GROUP_DATA[mGroupsCount] : null);
        mpGroups = new GROUP_DATA[mGroupsCount];

        pAiScript.StrCpy(out mpAiScript);
        pAi.StrCpy(out mpAi);

        mpTitle = string.Empty;
        if (pTitle != null) //STUB! Переделать на корректный импорт пустой строки
        {
            pTitle.StrCpy(out mpTitle);
        }

        // добавляем группы миссии
        h = 0;
        mGroupsCount = 0;
        List<string> MissionGroups = new List<string>();
        string pGroupName = string.Empty;
        while (true)
        {
            h = pGroups.GetNextHandle(h);
            if (h == 0) break;
            pGroupName = pGroups.GetNameByHandle(ref pGroupName, h);
            UniVarContainer pGroup = (UniVarContainer)pGroups.GetVariableTpl<iUnifiedVariableContainer>(h);
            if (pGroup == null) continue;
            mpGroups[mGroupsCount] = new GROUP_DATA();
            mpGroups[mGroupsCount].Init(pGroup, pGroupName);
            mGroupsCount++;
            MissionGroups.Add(pGroupName);
        }

        // добавляем группы из патча миссии
        if (patch)
        {
            List<GROUP_DATA> tmpGroups = new List<GROUP_DATA>(mpGroups);
            foreach (GROUP_DATA patch_group in missionPatch.Groups)
            {
                tmpGroups.Add(patch_group);
            }
            mGroupsCount = (uint)tmpGroups.Count;
            mpGroups = tmpGroups.ToArray();
        }
        // добавляем группы из Default Mission

        h = 0;
        mDefGroupsCount = 0;
        pGroupName = string.Empty;
        while (true)
        {
            h = pDefGroups.GetNextHandle(h);
            if (h == 0) break;
            pGroupName = pDefGroups.GetNameByHandle(ref pGroupName, h);
            if (MissionGroups.Contains(pGroupName)) continue;

            UniVarContainer pGroup = (UniVarContainer)pDefGroups.GetVariableTpl<iUnifiedVariableContainer>(h);

            if (pGroup == null) continue;
            mpDefGroups[mDefGroupsCount] = new GROUP_DATA();
            mpDefGroups[mDefGroupsCount].Init(pGroup, pGroupName);
            mpDefGroups[mDefGroupsCount].InCampaign();
            //Debug.Log($"Group {pGroupName} Handle {h} units {mpDefGroups[mDefGroupsCount].Units.Length}");
            mDefGroupsCount++;


        }

        // читаем маркеры
        if (pMarkers != null)
        {
            mMarkersCount = pMarkers.GetSize();
            Debug.Log("Loading " + mMarkersCount + " markers");
            if (mMarkersCount != 0)
            {
                mpMarkers = new MARKER_DATA[mMarkersCount];
                mMarkersCount = 0;
                while ((h = pMarkers.GetNextHandle(h)) != 0)
                {
                    //char pMarkerName[MAX_PATH];
                    string pMarkerName = null;
                    pMarkers.GetNameByHandle(ref pMarkerName, h);
                    Debug.Log("Loading marker " + pMarkerName + " " + h.ToString("X8"));
                    iUnifiedVariableVector pMarker = pMarkers.GetVariableTpl<iUnifiedVariableVector>(h);
                    if (pMarker == null) continue;
                    mpMarkers[mMarkersCount] = new MARKER_DATA();
                    mpMarkers[mMarkersCount++].Init(pMarker, pMarkerName);
                    // проверяем на спец. имя
                    if (pMarkerName[0] != '#') continue;
                    int Side;
                    switch (pMarkerName[1])
                    {
                        case 'H': Side = CS_SIDE_HUMANS; break;
                        case 'V': Side = CS_SIDE_VELIANS; break;
                        case 'N': Side = CS_SIDE_NEUTRAL; break;
                        case 'A': Side = CS_SIDE_ALIENS; break;
                        default: continue;
                    }
                    // обработываем спец. маркер
                    float r2 = Mathf.Pow(mpMarkers[mMarkersCount - 1].Radius, 2);
                    float x = mpMarkers[mMarkersCount - 1].x;
                    float z = mpMarkers[mMarkersCount - 1].z;
                    for (int i = 0; i < mDefGroupsCount; i++)
                    {
                        if (mpDefGroups[i].Side == CS_SIDE_NEUTRAL) continue;
                        if (r2 < Mathf.Pow(mpDefGroups[i].CenterX - x, 2) + Mathf.Pow(mpDefGroups[i].CenterZ - z, 2)) continue;
                        mpDefGroups[i].Side = (uint)Side;
                    }
                }
            }
        }
        //TODO - что-то в это есть неправильное!

        pMsn.GetBlockDWORD("EnabledCrafts", out myEnabledCrafts);
        pMsn.GetBlockDWORD("EnabledWeapons", out myEnabledWeapons);
    }
    //~AiMissionData();


    //c# 
    private T FindHashedData<T>(T[] pDatas, DWORD DatasCount, DWORD ID)
    {
        foreach (AIData data in pDatas)
        {
            if (data.GetID() == ID) return (T)data;
        }
        return default;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"Mission: [{mpTitle}]\n");
        sb.Append($"Default groups count: [{mDefGroupsCount}]\n");
        sb.Append($"Mission groups count: [{mGroupsCount}]\n");
        return sb.ToString();
    }
}
