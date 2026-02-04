using crc32 = System.UInt32;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;

public class MessageProcessor : IMessageProcessor
{
    IMissionAi mpMission;

    List<RadioMessageSaver> myMessages = new List<RadioMessageSaver>();
    List<RadioMessageInfo> myInfos = new List<RadioMessageInfo>();
    List<IGroupAi> myAis = new List<IGroupAi>();

    public MessageProcessor()
    {
        mpMission = null;
    }

    public bool Initialize(IMissionAi miss)
    {
        mpMission = miss;
        Assert.IsNotNull(miss);
        return true;
    }

    public void Destroy() { }

    IAi getIAi(int idx) { return myAis[idx] !=null ? (IAi)myAis[idx] : (IAi)mpMission; }
    IMessageProcessor GetIMessageProcessor() { return this; }

    // API
    public void AddMessage(RadioMessage Info, RadioMessageInfo rpm,IGroupAi grp)
    {
        Debug.Log("Adding message " + Info +" : " +  rpm);
        //dprintf("adding=%x\n",Info.Code);
        if (rpm.myWaitTime <= 0f || rpm.mySay == false)
            mpMission.ProcessRadioMessage(null, grp!=null ? (IAi)grp : (IAi)mpMission, Info, rpm.myToAll, false);
        else
        {
            //myInfos.New() = new RadioMessageInfo(rpm);
            //myMessages.New() = new RadioMessageSaver(Info);
            //myAis.New() = grp;
            myInfos.Add(rpm);
            myMessages.Add(new RadioMessageSaver(Info));
            myAis.Add(grp);
        }

    }
    public bool presentMessage(IGroupAi ai, crc32 callsign_name, crc32 msg, bool sender, int index = -1)
    {
        for (int i = 0; i < myInfos.Count; i++)
        {
            RadioMessage rm = myMessages[i].getMessage();
            string testCallsign = sender ? myMessages[i].CallerCallsign : myMessages[i].RecipientCallsign;
            int testIndex = sender ? rm.CallerIndex : rm.RecipientIndex;
            bool b1 = ai == myAis[i] && msg == rm.Code && Hasher.HshString(testCallsign) == callsign_name;
            if (b1 && (index == -1 || testIndex == index)) return true;
        }
        return false;

    }
    public void Update(float scale)
    {
        bool rfree = mpMission.RadioChannelIsFree();
        for (int i = 0; i < myInfos.Count;)
        {
            RadioMessageInfo info = myInfos[i];
            RadioMessageSaver message = myMessages[i];
            MessageProcessResult mpr = info.update(scale, rfree);
            //Debug.Log("MessageProcessor message " + message.getMessage() + " mpr " + mpr);
            switch (mpr)
            {
                case MessageProcessResult.rtCritical:
                case MessageProcessResult.rtSendAndExit:
                    if (mpr == MessageProcessResult.rtCritical && !info.mySayIfExceed)
                    {
                        message.getMessage().mFlags &= RadioMessage.RMF_SAY;
                    }
                        //const_cast<RadioMessage*>(message->getMessage())->mFlags &= ~RMF_SAY;
                    mpMission.ProcessRadioMessage(null, getIAi(i), message.getMessage(), info.myToAll, true/*mpr==rtSendAndExit || (mpr==rtCritical&& info->mySayIfExceed==true)*/);
                    remove(i);
                    return;
                case MessageProcessResult.rtExit:
                    //dprintf("removing=%x\n",message->getMessage()->Code);
                    remove(i);
                    break;
                case MessageProcessResult.rtContinue:
                    i++;
                    break;
            }

        }

    }
    public void onGroupDestroy(IGroupAi ai)
    {
        for (int i = 0; i < myAis.Count; ++i)
            if (myAis[i] == ai)
                remove(i--);
    }

    void remove(int i)
    {
        //myInfos.erase(i);
        //myMessages.erase(i);
        myInfos.RemoveAt(i);
        myMessages.RemoveAt(i);
        myAis.RemoveAt(i);
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }
}
