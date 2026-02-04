using UnityEngine;

public interface IFont : IObject
{

    public void SetCustomColor(int num, Color color);
    public Color GetCustomColor(int num);
    /// <summary>
    /// scales sizes
    /// </summary>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="spacing"></param>
    public void SetWH(float w, float h, float spacing = 0);
    /// <summary>
    /// horizontal text size in local coordinates
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="Len"></param>
    /// <returns></returns>
    public float Width(string Text, int Len = 0x7FFFFFFF);
    /// <summary>
    /// horizontal char size in local coordinates
    /// </summary>
    /// <param name="Ch"></param>
    /// <param name="WithSpasing"></param>
    /// <returns></returns>
    public float Width(char Ch, bool WithSpasing = true);
    /// <summary>
    /// vertical text size in local coordinates
    /// </summary>
    /// <returns></returns>
    public float Height();

    public void Puts(Color Color, Vector2 p, string Text);        // *
    public void Printf(Color Color, Vector2 p, string Text, params object[] args);  // *

    //protected :
    //public ~IFont() { };
};