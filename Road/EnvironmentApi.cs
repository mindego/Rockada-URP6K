using UnityEngine;
using static IDataHasher;
public static class EnvironmentApi
{
    public static IDataHasher CreateDH(int _poolsize, IRoadsStore rs, ILog log, int v = (int)DATA_HASHER_VERSION)
    {
        return v == DATA_HASHER_VERSION ? (IDataHasher)
            new DataHasher(rs, _poolsize, log) : null;
    }

    public static INavigation CreateNS(IDataHasher _srv, TERRAIN_DATA _trn, ILog _log, uint v = INavigation.NAVIGATION_VERSION)
    {
        return v == INavigation.NAVIGATION_VERSION ?
            //new NavigationFake(_srv, _trn, _log) : null; //TODO заменить на реальную навигацию
        new Navigation(_srv, _trn, _log) : null; 
    }

    public static IRoadBuilder CreateRB(IRoadsStore rs, int v=IRoadBuilder.ROAD_BUILDER_VERSION)
    {
        return v == IRoadBuilder.ROAD_BUILDER_VERSION ?
           new RoadBuilder(rs) : null;
    }
}

