using UnityEngine;

public interface iBaseColliding : iBaseInterface
{
   new  public const uint ID = 0xA0DB5A60; // iBaseInterface
    /// <summary>
    /// is ready to collide
    /// </summary>
    /// <returns></returns>
    public bool IsReady();
    /// <summary>
    /// unit's main FPO
    /// </summary>
    /// <returns></returns>
    public FPO GetFpo();
    /// <summary>
    /// where this unit was last seen
    /// </summary>
    /// <returns></returns>
    public Vector3 GetOrg();
    /// <summary>
    /// orientation in world (vectors)
    /// </summary>
    /// <returns></returns>
    public Vector3 GetDir();
    /// <summary>
    /// orientation in world (vectors)
    /// </summary>
    /// <returns></returns>
    public Vector3 GetUp();
    /// <summary>
    /// orientation in world (vectors)
    /// </summary>
    /// <returns></returns>
    public Vector3 GetRight();
    /// <summary>
    /// physical radius 
    /// </summary>
    /// <returns></returns>
    public float GetRadius();
    public float GetWeight();
    public float GetMaxSpeed();
    /// <summary>
    /// speed
    /// </summary>
    /// <returns></returns>
    public Vector3 GetSpeed();
    /// <summary>
    /// speed
    /// </summary>
    /// <param name="Org"></param>
    /// <returns></returns>
    public Vector3 GetSpeedFor(Vector3 Org);
    /// <summary>
    /// отмотаться назад
    /// </summary>
    /// <param name="scale"></param>
    public void Rewind(float scale);
    /// <summary>
    /// один шаг интерполяции
    /// </summary>
    /// <param name="scale"></param>
    public void MakeStep(float scale);  // один шаг интерполяции
    /// <summary>
    /// силовое воздействие
    /// </summary>
    /// <param name="Force"></param>
    /// <param name="Org"></param>
    public void ApplyForce(Vector3 Force, Vector3 Org); // силовое воздействие
};