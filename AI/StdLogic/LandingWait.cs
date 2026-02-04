using UnityEngine;

class LandingWait<T> where T:ContactHolder
{
    const float HangarQueryCancelTime = 30f;
    public enum Status
    {
        WAIT,
        ABORTED,
        COMPLETE,
        DEAD
    };
    public LandingWait(T contact) { 
        myContact = contact;
        /*myStatus( WAIT ),*/
        myTime = 0f;
        contact.AddRef();
    }

    public Status updateStatus(float time)
    {
        Status myStatus = Status.WAIT;
        myTime += time;
        if (myContact.isDead())
            myStatus = Status.DEAD;
        else
        if (myContact.isManual() == false)
            myStatus = Status.COMPLETE;
        else
          if (myTime >= HangarQueryCancelTime)
            myStatus = Status.ABORTED;

        Debug.Log("Landing status: " + myStatus);
        return myStatus;
    }

    public T getContact() { return myContact; }
    private T myContact;
    float myTime;
};
