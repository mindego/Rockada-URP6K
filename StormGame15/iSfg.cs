using UnityEngine;
/// <summary>
/// interface for Supressing fields generators
/// </summary>
public interface iSfg
{
    public const uint ID = 0x161E3059;
    /// <summary>
    /// центр поля в мировых координатах
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCenter();  // центр поля в мировых координатах
    /// примерный радиус поля
    public float GetRadius();  // примерный радиус поля
    /// <summary>
    /// включено ли?
    /// </summary>
    /// <returns></returns>
    public bool IsOn();  // включено ли?
    /// <summary>
    /// в зоне действия?
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public bool IsInRange(Vector3 o);// в зоне действия?
    /// <summary>
    /// в зоне действия?
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public bool IsCameraInRange(Vector3 o);// в зоне действия?

    /// <summary>
    /// включить
    /// </summary>
    public void TurnOn(); // включить
    /// <summary>
    /// выключить
    /// </summary>
    public void TurnOff(); // выключить

    /// <summary>
    /// показать
    /// </summary>
    /// <param name="showOn"></param>
    public void show(bool showOn); // показать
};

