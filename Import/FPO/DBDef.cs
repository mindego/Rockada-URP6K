public class DBDef
{

    // Return values for DB::Open(...)
    public const int DB_OK = 0;
    public const int DB_CANT_OPEN = 1;
    public const int DB_BAD_FORMAT = 2;
    public const int DB_FAILED_LOAD = 3;

    // Return values for DB_PROC Load
    public const int DB_LOAD_FAILED = 0;
    public const int DB_LOAD_OK = 1;
    /// <summary>
    /// delete data immediently
    /// </summary>
    public const int DB_LOAD_OK_EX = -1; // delete data immediently

    // Return values for DB_PROC Free
    public const int DB_FREE_OK = 1;
    /// <summary>
    /// must return if LoadProc return DB_LOAD_OK_EX
    /// </summary>
    public const int DB_FREE_OK_EX = -1; // must return if LoadProc return DB_LOAD_OK_EX

    // Return values for enumeration function
    public const int DB_ENUM_OK = 0;
    /// <summary>
    /// or any nonzero
    /// </summary>
    public const int DB_ENUM_FAIL = 1; // or any nonzero

    // Load type
    public const bool DATA_LOAD_DYNAMIC = true;
    public const bool DATA_LOAD_STATIC = false;

}