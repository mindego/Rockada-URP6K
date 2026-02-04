public class TMatrix<T> where T:new()
{
    protected readonly int height;
    protected readonly int width;
    private T[] elems;
    public TMatrix(int h, int w)
    {
        height = h;
        width = w;
        elems = new T[w * h];

        for (int i = 0;i<h*w;i++)
        {
            elems[i] = new T();
        }
    }
    ~TMatrix() { elems = null; }

    public T this[int i] { get { return elems[i * width]; } } //todo скорее всего - не так. Нужен массив-столбец
    public T this[int x, int y]
    {
        get
        {
            return elems[y * width + x];
        }
        set { 
            elems[y * width + x] = value;
        }
    }
    //    T operator [] (int i ) { return elems+i* width;
    //}
    public int Height() { return height; }
    public int Width() { return width; }
};
