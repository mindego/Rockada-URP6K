using DWORD = System.UInt32;
/// <summary>
/// interface for hangars
/// </summary>
public interface iSubHangar
{
    public const uint CAN_ACCEPT_NONE = 0x0;
    public const uint CAN_ACCEPT_LAND = 0x1;
    public const uint CAN_ACCEPT_TAKEOFF = 0x2;
    public const uint CAN_ACCEPT_ALL = (CAN_ACCEPT_LAND | CAN_ACCEPT_TAKEOFF);

    public const uint ID=0x9114C627;
    /// <summary>
    /// открыта ли дверь анагра?
    /// </summary>
    /// <returns></returns>
    public bool isDoorOpened();
    /// <summary>
    /// закрыта ли дверь анагра?
    /// </summary>
    /// <returns></returns>
    public bool isDoorClosed();
    /// <summary>
    /// может ли работать с данным unit-ом (по CodedName)?
    /// </summary>
    /// <param name="CodedName"></param>
    /// <returns></returns>
    public int canHandleUnit(DWORD CodedName);// может ли работать с данным unit-ом (по CodedName)?
    /// <summary>
    /// может ли работать с данным unit-ом?
    /// </summary>
    /// <returns></returns>
    public int canHandleUnit(iContact pUnit);
    public iContact getIContact();
};