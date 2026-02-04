using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// Интерфейс управления наземной техникой
/// </summary>
public interface iMovementSystemCarrier
{
    public const DWORD ID = 0xF11AAF27;

    // управление автопилотом
    public void MoveTo(Vector3 v, float time, float max_speed);
    public void NearUnit(iContact i, Vector3 Delta);
    public void Pause(bool pause);
    public bool IsStopped();
};