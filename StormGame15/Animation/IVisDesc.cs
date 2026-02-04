//using static AnimationData;
using System;

public interface IVisDesc : IDisposable
{
    void activate(IAnimationSupport sup);
    void deactivate(IAnimationSupport sup);
    void destroy(IAnimationSupport sup);
    bool onDestroyFpo(FPO fpo);
    void update(IAnimationSupport sup);
}