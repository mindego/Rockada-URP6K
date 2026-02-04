class Landing<T> where T: ContactHolder
{
    //typedef T Contact;
    public enum Status
    {
        IN_PROGRESS,
        ABORTED,
        COMPLETE
    };
    public Landing(T contact) {
        myContact = contact;
        myStatus = Status.IN_PROGRESS;
        contact.AddRef();
    }
    public Status updateStatus()
    {
        if (myContact.isManual())
            myStatus = Status.ABORTED;
        if (myContact.isLandingComplete())
            myStatus = Status.COMPLETE;
        return myStatus;
    }

    public T getContact() { return myContact; }
    private T myContact;
    Status myStatus;
};