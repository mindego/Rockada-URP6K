using DWORD = System.UInt32;
public struct AwardInfo
{
    public string mpName;
    public bool mTwice;

    public AwardInfo(string mpName, bool mTwice)
    {
        this.mpName = mpName;
        this.mTwice = mTwice;
    }
};


public partial class StdCooperativeAi
{
    public const int AWARDS_COUNT = 18;
    public const int AWARDSNIPER3 = 0;
    public const int AWARDSNIPER2 = 1;
    public const int AWARDSNIPER1 = 2;
    public const int AWARDFALCON3 = 3;
    public const int AWARDFALCON2 = 4;
    public const int AWARDFALCON1 = 5;
    public const int AWARDBOOT3 = 6;
    public const int AWARDBOOT2 = 7;
    public const int AWARDBOOT1 = 8;
    public const int DIAMONDBOOT3 = 9;
    public const int DIAMONDBOOT2 = 10;
    public const int DIAMONDBOOT1 = 11;
    public const int BESTFIGHTER3 = 12;
    public const int BESTFIGHTER2 = 13;
    public const int BESTFIGHTER1 = 14;
    public const int HANDICAPFIGHTER = 15;
    public const int STARMONSTER = 16;
    public const int CHIPANDDAIL = 17;

    static readonly AwardInfo[] mAwards = new AwardInfo[AWARDS_COUNT]{
        new AwardInfo("award_Sniper3"     , true ),   // 0    
	    new AwardInfo( "award_Sniper2"     , true  ),   // 1    
	    new AwardInfo( "award_Sniper1"     , true  ),   // 2    
	    new AwardInfo( "award_Falcon3"     , false ),   // 3    
	    new AwardInfo( "award_Falcon2"     , false ),   // 4    
	    new AwardInfo( "award_Falcon1"     , false ),   // 5    
	    new AwardInfo( "award_Boot3"       , true  ),   // 6    
	    new AwardInfo( "award_Boot2"       , true  ),   // 7    
	    new AwardInfo( "award_Boot1"       , true  ),   // 8    
	    new AwardInfo( "award_DiamondBoot3", false ),   // 9    
	    new AwardInfo( "award_DiamondBoot2", false ),   // 10   
	    new AwardInfo( "award_DiamondBoot1", false ),   // 11   
	    new AwardInfo( "award_BestFighter3", false ),   // 12   
	    new AwardInfo( "award_BestFighter2", false ),   // 13   
	    new AwardInfo( "award_BestFighter1", true  ),   // 14   
	    new AwardInfo( "award_HandFighter" , true  ),   // 15   
	    new AwardInfo( "award_StarMonster" , true  ),   // 16   
	    new AwardInfo( "award_ChipAndDail" , true  )    // 17   
    };

    public void AddAward(string name)
    {

        DWORD hs = Hasher.HshString(name);
        for (int i = 0; i < AWARDS_COUNT; ++i)
            if (Hasher.HshString(mAwards[i].mpName) == hs)
            {
                //SetAward(mpAwards, i, 1); //TODO Вернуть наградную систему!
                break;
            }
    }

    const string scol_Count = "col_Count";
    void CreateAwardsList(iUnivarTable awards)
    {
        if (awards.IsNew())
            awards.AddColoumn(scol_Count, iUnifiedVariableInt.ID);
    }
}
