public class RadioMessageSaver
{
    public string CallerCallsign;
    public string RecipientCallsign;
    public readonly TContact TargetContact = new TContact();
    public string String1;
    public string String2;
    RadioMessage myMsg;

    public RadioMessage getMessage() { return myMsg; }

    public RadioMessageSaver(RadioMessage msg)
    {
        myMsg = msg;
        CallerCallsign = "";
        RecipientCallsign = "";
        String1 = "";
        String2 = "";
        // copy
        //TargetContact = new TContact(msg.TargetContact);
        TargetContact.setPtr(msg.TargetContact);
        CallerCallsign = backupString(msg.CallerCallsign);
        RecipientCallsign = backupString(msg.RecipientCallsign);
        String1 = backupString(msg.String1);
        String2 = backupString(msg.String2);
        // redirect
        myMsg.String1 = String1;
        myMsg.String2 = String2;
        myMsg.RecipientCallsign = RecipientCallsign;
        myMsg.CallerCallsign = CallerCallsign;
    }
    //не думаю, что это надо. Но сохранить можно для совместимости.
    private string backupString(string from) { return new string(from); }

};

