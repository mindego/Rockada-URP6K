using System.IO;

public interface IStormImportable<T>
{
    public T Import(Stream st);
    //public T GetDefault();
}

public interface ISizeEvaluator
{
    public static int GetSize()
    {
        throw new System.NotImplementedException();
    }
}
