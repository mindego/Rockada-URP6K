using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

public class ContactInfoHolder : IContactInfo
{

    public virtual ContactData getData(int i)
    {
        switch (myLastScan)
        {
            case ScanArea.saEnemies: return i < myEnemies.Count ? (myEnemies[i]) : null;
            case ScanArea.saNeutrals: return i < myNeutrals.Count ? (myNeutrals[i]) : null;
            case ScanArea.saFriends: return i < myFriends.Count ? (myFriends[i]) : null;
        }
        return null;
    }

    int refreshFriends(float radius)
    {
        return processContacts(true, radius);
    }
    int refreshNeutrals(float radius)
    {
        return processContacts(false, radius);
    }

    public virtual int refresh(ScanArea area, float radius)
    {
        myLastScan = area;
        switch (myLastScan)
        {
            case ScanArea.saEnemies: return myEnemies.Count;
            case ScanArea.saNeutrals: myNeutrals.Clear(); return refreshNeutrals(radius);
            case ScanArea.saFriends: myFriends.Clear(); return refreshFriends(radius);
        }
        return 0;
    }

    public void clearEnemies()
    {
        myEnemies.Clear();
    }

    public void addEnemy(crc32 en_name, Vector3 dist2)
    {
        addContact(en_name, dist2, ref myEnemies);
    }

    void addContact(crc32 en_name, Vector3 dist3, ref List<ContactData> tab)
    {
        ContactData info = new ContactData
        {
            myName = en_name,
            myDiff = dist3
        };
        tab.Add(info);
    }

    void addFriend(crc32 en_name, Vector3 dist2)
    {
        addContact(en_name, dist2, ref myFriends);
    }

    void addNeutral(crc32 en_name, Vector3 dist2)
    {
        addContact(en_name, dist2, ref myNeutrals);
    }

    int processContacts(bool friendly, float radius)
    {
        iSensors sns = myAi.GetLeaderSensors();
        if (sns!=null)
        {
            Vector3 leader_org = myAi.GetLeaderOrg();
            iContact cur = null;
            do
            {
                cur = friendly ? sns.GetFriend(cur) : sns.GetEnemyInZone(leader_org, radius, cur, 0, true);
                DWORD grp_id;
                DWORD un_id;
                DWORD side;
                if (cur!=null && myAi.mpMission.GetContactInfo(cur, out grp_id, out un_id, out side))
                    if (myAi.mpData.ID != grp_id)
                    {
                        if (friendly)
                            addFriend(grp_id, cur.GetOrg() - leader_org);
                        else
                            addNeutral(grp_id, cur.GetOrg() - leader_org);
                    }
            } while (cur!=null);
        }
        return friendly ? myFriends.Count : myNeutrals.Count;

    }

    public ContactInfoHolder(StdGroupAi ai)
    {
        myAi = ai;
    }

    StdGroupAi myAi;
    ScanArea myLastScan;

    List<ContactData> myEnemies = new List<ContactData>();
    List<ContactData> myNeutrals = new List<ContactData>();
    List<ContactData> myFriends = new List<ContactData>();
};
