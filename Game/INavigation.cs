using UnityEngine;

public interface INavigation : IRefMem
{
    public const uint NAVIGATION_VERSION = 0x00010001;

    public float GetSquareSize();
    public bool SquareIsFree(Vector3 pos);
    public INavigationOrder CalcOrder(Vector3 src, Vector3 dst, bool use_global);
    public void CancelOrder(INavigationOrder ino);
    public void Initialize(float calc_time, float local_calc_radius);
    public void Update(float scale);
};
