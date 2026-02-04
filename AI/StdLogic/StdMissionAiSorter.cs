using WORD = System.UInt16;

public partial class StdMissionAi
{
    public void Synchronize(MissionClient client, CDWrapperTable tab, iGetTableInfoClient gcl)
    {
        WORD rows_count = GetFragRowsCount();
        if (rows_count > 0)
        {
            //WORD rows = (WORD*)alloca(sizeof(WORD) * rows_count);
            WORD[] rows =new WORD[rows_count];
            if (FillFragRows(rows_count, rows))
            {
                tab.Synchronize(client, rows_count, rows, gcl);
            }
        }
    }

}