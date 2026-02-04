using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// Интерфейс управления наземной техникой
/// </summary>
interface iMovementSystemVehicle : iMovementSystem
{
    new public const DWORD ID = 0xADA3BABD;

    // управление автопилотом
    public void FollowUnit(iContact f, float dist);
    public void NearUnit(iContact f, float dist, Vector3 delta);
    public void MoveTo(Vector3 v, bool use_roads, float time);
    public void Manual();
    public bool Land(iContact h);

    // пауза
    public void Pause(bool pause);
    public bool IsPaused();

    // непосредственное управление
    public void SetStick(float v);
    public void SetThrust(float v);
    public float GetStick();
    public float GetThrust();

    // достигли ли мы точки
    public bool GetReachedFlag();

    // очистить маршрут
    public void ClearRoute();

    new public bool IsManual() { return false; }

    public void setMaxSpeed(float speed);
};
