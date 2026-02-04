using crc32 = System.UInt32;
using static RoFlags;
using static HashFlags;

public class LocationHolder : ILocationHolder, IRoadsStore
{
    public LoadResult loadLocation(ILocation loc)
    {
        LoadResult ret = LoadResult.LR_SUCCESS;
        crc32 name = loc.getID();
        if (myLocationName != name)
        {
            closeLocation();
            ret = openLocation(loc);
            if (LoadResult.LR_SUCCESS == ret)
            {
                myLocationName = name;
                //myLocation = SafeAddRef(loc);
                myLocation = loc;
            }
            else
                closeLocation();
        }
        return ret;
    }

    ~LocationHolder()
    {
        closeLocation();
    }

    ILocation myLocation;

    public LocationHolder(RendererApi r)
    {
        myLocationName = CRC32.CRC_NULL;
        myTerrain = null;
        myRenderer = r;
    }

    public void closeLocation()
    {
        myNavigation = null;
        myDataHasher = null;
        myFpoLoader = null;
        myCollision = null;
        myHash = null;

        if (myTerrain != null)
        {
            myTerrain.Close();
            //delete myTerrain;
            myTerrain = null;
        }
        myLocationName = CRC32.CRC_NULL;
        myLocation = null;

    }
    LoadResult openLocation(ILocation loc)
    {
        if (!openTerrain(loc.getTerrainName()))
            return LoadResult.LR_NO_TERRAIN;

        openHash();
        openCollision();

        if (!openObjects())
            return LoadResult.LR_NO_OBJECTS;

        openRoadSystem(loc);

        return LoadResult.LR_SUCCESS;
    }

    bool openTerrain(string name)
    {
        name = ProductDefs.GetPI().getHddFile(name);
        myTerrain = new TERRAIN_DATA();
        return (myTerrain.Open(name) &&
            myTerrain.OpenSq(true, false) &&
            myTerrain.OpenBx(true, false) &&
            myTerrain.OpenVb(true, false));
    }

    bool openObjects()
    {
        myFpoLoader = FpoLoader.CreateFpoLoader(myRenderer, myCollision, "objects2.dat", "objects.dat");
        return myFpoLoader != null;
    }

    void openHash()
    {
        myHash = IHashApi.CreateHasher2(IHashApi.HASH_VERSION, myTerrain, 400000f, 512f, 1024 * 300, null, false);
        myHash.SetSecondCache(ROObjectId(ROFID_LIGHT));
    }
    void openCollision()
    {
        myCollision = CollisionModuleAPI.CreateCl(CollisionDefines.COLL_VERSION, myHash, myTerrain);
    }
    void openData() { } // TODO реализовать загрузку данных в создании LocationHolder
    void openRoadSystem(ILocation loc)
    {
        myDataHasher = EnvironmentApi.CreateDH(20480 * 2, this, null);
        myDataHasher.HashRoads(loc.getRoadNet());
        //TODO создавать и инициализировать систему навигации в локации
        myNavigation = EnvironmentApi.CreateNS(myDataHasher, myTerrain, null);
        myNavigation.Initialize(0.2f, 4096);
    }

    crc32 myLocationName;
    RendererApi myRenderer;

    TERRAIN_DATA myTerrain;
    INavigation myNavigation;
    IDataHasher myDataHasher;
    IFpoLoader myFpoLoader;
    ICollision myCollision;
    IHash myHash;

    public TERRAIN_DATA getTerrain()
    {
        return myTerrain;
    }
    public INavigation getNavigation()
    {
        return myNavigation;
    }
    public IDataHasher getDataHasher()
    {
        return myDataHasher;
    }
    public IMappedDb getGameDatas()
    {
        return stormdata_dll.getGameData();
    }
    public IFpoLoader getFpoLoader()
    {
        return myFpoLoader;
    }
    public ICollision getCollision()
    {
        return myCollision;
    }
    public IHash getHash()
    {
        return myHash;
    }

    public ILocation getCurrentLocation()
    {
        return myLocation;
    }

    // for hata hasher
    public ROADDATA getByCode(crc32 code, bool mustExists = true)
    {
        return ROADDATA.GetByCode(code, mustExists);
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }
}