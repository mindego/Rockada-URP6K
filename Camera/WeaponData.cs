using System;

public class WeaponData : TLIST_ELEM<WeaponData>,IDisposable
{
    public int colour;
    public string text;
    public float value;
    public int tape;
    public bool box;
    public bool current;

    public override string ToString()
    {
        string res = string.Format("{0} text: {1} val: {2} tape: {3} box: {4} current {5} colour {6}",
                GetType().ToString(),
                text,
                value,
                tape,
                box.ToString(),
                current.ToString(),
                colour
            );
        return res;
    }
    public WeaponData(int _colour,string  _Text,float _value,bool _box,int _tape) {

        colour = _colour;
        value = _value;
        box = _box;
        tape = _tape;
        current = (false);

        text = _Text.Length < 16? _Text:_Text.Substring(0, 16);
        text.Trim();
        //text[sizeof(text) - 1] = 0;
        //StrnCpy(text, _Text, sizeof(text));


    }

    WeaponData prev, next;
    public WeaponData Next()
    {
        return next;
    }

    public WeaponData Prev()
    {
        return prev;
    }

    public void SetNext(WeaponData t)
    {
        next = t;
    }

    public void SetPrev(WeaponData t)
    {
        prev = t;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}