//#define DAMAGE_REPORT

using System;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.GridLayoutGroup;
/// <summary>
/// BaseActor: класс, кторому регулярно вызывается Update(float scale) и который удалаяется,
/// если его Update вернул false
/// </summary>
public class BaseActor : iBaseInterface, TLIST_ELEM<BaseActor>
{
    //от TLIST
    private BaseActor next, prev;

    public BaseActor Next()
    {
        return next;
    }
    public BaseActor Prev()
    {
        return prev;
    }
    public void SetPrev(BaseActor _prev)
    {
        prev = _prev;
    }
    public void SetNext(BaseActor _next)
    {
        next = _next;
    }

    //от 
    public BaseScene rScene;
    public const uint ID = 0x33FCBA4A;
    iBaseActor owner;
    public BaseActor(BaseScene rScene)
    {
        
        this.rScene = rScene;
        rScene.ActorsList.AddToTail(this);
        //owner = _owner;
        //Debug.Log("Adding in ActorsList " + _owner);
        //rScene.ActorsList.AddToTail(_owner);
        //rScene.ActorsList.AddToTail(this);
        //rScene.ActorsList.AddLast(this);
        //rScene.Message("Created BaseActor {0} total {1}", this.ToString(), rScene.ActorsList.Count.ToString());
    }

    public void SetOwner(iBaseActor _owner)
    {
        owner = _owner;
        rScene.Message("Adding BaseActor {0} total {1} from BaseScene", this.ToString() + "/" + owner, rScene.ActorsList.Counter().ToString());
    }

    ~BaseActor()
    {
        Dispose();
    }

    private bool BaseActorDisposed = false;
    public virtual void Dispose()
    {
        if (rScene.ActorsList == null) return;
        if (BaseActorDisposed) return;
        rScene.Message("Disposing of BaseActor {0} total {1} from BaseScene", this.ToString() + "/"+ owner, rScene.ActorsList.Counter().ToString());
        rScene.ActorsList.Sub(this);
        BaseActorDisposed = true;

        //owner.Dispose();
    }

    /// <summary>
    /// <br>Метод движения игрового объекта.
    /// Метод возвращает true, если объект смог двигаться. Возвращается именно _возможность_ двигаться - "двигаться" могут и здания (оставаясь на места)
    /// Столкновения, вызывающие невозможность двигаться  (к примеру - упёрся в стену) всё равно приводят к возврату true
    /// False обычно возвращается, если объект "умер"
    /// </br>
    /// <br>В оригинале все игровые объекты наследуются от BaseActor и реализуют свой собственный метод Move, но из-за множественного наследования приходится делать так</br>
    /// </summary>
    /// <param name="scale"></param>
    /// <returns>true, если движение удалось и false, если нет</returns>
    virtual public bool Move(float scale) {

        return owner != null ? owner.Move(scale) : false;
    }
    virtual public void Update(float scale) {
        if (owner!=null) owner.Update(scale);
    }

    public virtual object GetInterface(uint var)
    {
        throw new NotImplementedException();
    }

    public virtual T GetInterface<T>() where T : iBaseInterface
    {
        throw new NotImplementedException("Use (class)GetInterface(class.ID) instead");
    }
}

