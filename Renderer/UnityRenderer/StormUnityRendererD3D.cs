using UnityEngine;
using UnityEngine.UIElements;
using WORD = System.UInt16;

public class StormUnityRendererD3D
{
    private StormUnityPolyDecalDraw drawPolyDecal;
    public StormUnityRendererD3D()
    {
        drawPolyDecal = new StormUnityPolyDecalDraw();
    }

    internal void DrawDecal(PolyDecal polyDecal)
    {
        drawPolyDecal.Draw(polyDecal);
    }

    public void Flush()
    {
        drawPolyDecal.Flush();
    }
}

public class DetailDebug : MonoBehaviour
{
    public Vector3 val1;
    public Vector3 val2;
}



