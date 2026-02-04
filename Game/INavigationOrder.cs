public interface INavigationOrder : IRefMem
{
    public NavigationOrderState GetState();
    public int GetRouteDimension();
    public ROADPOINT[] GetRouteBuffer();
};
