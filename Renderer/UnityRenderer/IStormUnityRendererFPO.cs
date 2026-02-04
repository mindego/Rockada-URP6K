using UnityEngine;

public interface IStormUnityRendererFPO
{
    void Draw(FPO myFPO);
    void Cleanup();

    //Transform GetCurrentTransform();
}