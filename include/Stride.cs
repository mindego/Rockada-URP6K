using Unity.VisualScripting;

public class Stride<T>
{
    private T[] ptr;
    private int stride;

    public Stride(T[] src_l, int _stride)
    {
        ptr = src_l;
        stride = _stride;
    }

    public T this[int i]
    {
        get => ptr[i];
        set => ptr[i] = value;
    }
};