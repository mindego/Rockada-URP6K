using DWORD = System.UInt32;

public class CoopScoreInfo
{
    DWORD myID;
    MissionClient mpClient;
    string myName;
    int myAir;
    int myGround;
    int myShip;
    int myVehicle;

    public void Clear() { myAir = myGround = myVehicle = myShip = 0; }


    public void addAir(int num) { myAir += num; }
    public void addGround(int num) { myGround += num; }
    public void addShip(int num) { myShip += num; }
    public void addVehicle(int num) { myVehicle += num; }

    public int getAir() { return myAir; }
    public int getGround() { return myGround; }
    public int getShips() { return myShip; }
    public int getVehicles() { return myVehicle; }

    public DWORD getID() { return myID; }
    public string getName() { return myName != null ? myName : mpClient.Name(); }

    public MissionClient GetClient() { return mpClient; }

    ~CoopScoreInfo()
    {
        if (myName != null)
            myName = null;
    }

    public CoopScoreInfo(MissionClient client) { myName = null; mpClient = client; myID = (uint)mpClient.ID(); Clear(); }
    public CoopScoreInfo(DWORD id, string name) { mpClient = null; myID = id; Clear(); myName = name; }
};