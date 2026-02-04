using crc32 = System.UInt32;

public interface ITriggerService
{
    public const uint ID = 0xC448F95E;
    public void setTrigger(string name, int value, bool global = false);
    public bool getTrigger(string name, out int value);
    public bool isTriggerGlobal(string name);
    public TriggerValue getTrigger(int i);
    public bool getTrigger(crc32 name, out int value);
};

public class TriggerValue
{
    public int myValue;
    public bool myGlobal;
    public string myName;
    public TriggerValue() { }
    public TriggerValue(string name, int val, bool gl) { 
        myName = name;
        myValue = val; 
        myGlobal = gl;  }
};
