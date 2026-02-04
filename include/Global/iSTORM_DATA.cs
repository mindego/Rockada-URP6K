using DWORD = System.UInt32;

public interface iSTORM_DATA<T>
{
    public T GetByCodeLocal(DWORD Code, bool MustExist = true);
    public static T GetByCode(DWORD Code, bool MystExist = true) { return default; }

}
