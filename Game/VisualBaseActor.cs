using System;

public class VisualBaseActor : IDisposable, TLIST_ELEM<VisualBaseActor>
{
    // взаимодействие с SceneVizualizer
    protected SceneVisualizer pVis;
    private TLIST_ELEM_IMP<VisualBaseActor> myTLIST_ELEM;

    public virtual bool Update(float scale) { return true; }

    private bool isDisposed = false;
    public virtual void Dispose()
    {
        if (isDisposed) return;
        pVis.VisualActorsList.Sub(this);
        UnityEngine.Debug.Log("Disposed of VisualBaseActor " + this + " : " + this.GetHashCode().ToString("X8") + " total " + pVis.VisualActorsList.Counter());
        isDisposed = true;
    }

    public VisualBaseActor Next()
    {
        return ((TLIST_ELEM<VisualBaseActor>)myTLIST_ELEM).Next();
    }

    public VisualBaseActor Prev()
    {
        return ((TLIST_ELEM<VisualBaseActor>)myTLIST_ELEM).Prev();
    }

    public void SetNext(VisualBaseActor t)
    {
        ((TLIST_ELEM<VisualBaseActor>)myTLIST_ELEM).SetNext(t);
    }

    public void SetPrev(VisualBaseActor t)
    {
        ((TLIST_ELEM<VisualBaseActor>)myTLIST_ELEM).SetPrev(t);
    }

    // создание/удаление
    public VisualBaseActor(SceneVisualizer v)
    {
        pVis = v;
        myTLIST_ELEM = new TLIST_ELEM_IMP<VisualBaseActor>();
        pVis.VisualActorsList.AddToTail(this);
    }

    ~VisualBaseActor()
    {
        Dispose();
    }


}
