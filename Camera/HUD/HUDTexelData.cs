using DWORD = System.UInt32;

public class HUDTexelData : BaseData
{
    public float[] GunSight = new float[4]; //[4]
    public float[] BigRing = new float[4]; //[4]
    public float[] Empty = new float[4]; //[4]
    public float[] CompasScale = new float[4]; //[4]
    //public float[,] CompasAzimut = new float[12, 4]; //[12][4]
    public float[][] CompasAzimut = new float[12][]; //[12][4]
    public float[] CompasBox = new float[4]; //[4]
    public float[] TargetNE = new float[4]; //[4]
    public float[] TargetSE = new float[4]; //[4]
    public float[] TargetSW = new float[4]; //[4]
    public float[] TargetNW = new float[4]; //[4]
    public float[] LineTop = new float[4]; //[4]
    public float[] LineLeft = new float[4]; //[4]
    public float[] LineRight = new float[4]; //[4]
    public float[] LineBottom = new float[4]; //[4]
    public float[] TargetUp = new float[4]; //[4]
    public float[] TargetDown = new float[4]; //[4]
    public float[] TargetLeft = new float[4]; //[4]
    public float[] TargetRight = new float[4]; //[4]
    public float[] AviaHor = new float[4]; //[4]
    public float[] AviaHor1 = new float[4]; //[4]
    public float[] AviaHor2 = new float[4]; //[4]
    public float[] LScale = new float[4]; //[4]
    public float[] HScale = new float[4]; //[4]
    public float[] RScale = new float[4]; //[4]
    public float[] OffScale = new float[4]; //[4]
    public float[] Box = new float[4]; //[4]
    public float[] LBracket = new float[4]; //[4]
    public float[] RBracket = new float[4]; //[4]
    public float[] Point = new float[4]; //[4]
    public float[] PointBlack = new float[4]; //[4]
    public float[] Craft = new float[4]; //[4]
    public float[] Tank = new float[4]; //[4]
    public float[] Turet = new float[4]; //[4]
    public float[] SeaShip = new float[4]; //[4]
    public float[] AirShip = new float[4]; //[4]
    public float[] Rocket = new float[4]; //[4]
    public float[] Select = new float[4]; //[4]
    //public float[,] Bar = new float[3, 4]; //[3][4]
    public float[][] Bar = new float[3][]; //[3][4]
    public float[] Bar100 = new float[4]; //[4]
    public int[] BarCount = new int[3]; //[3]
    public float[] WBar = new float[4]; //[4]
    public float[] WBox = new float[4]; //[4]
    public float[] TriangleFull = new float[4]; //[4]
    public float[] Triangle = new float[4]; //[4]
    public float[] LampA = new float[4]; //[4]
    public float[] LampT = new float[4]; //[4]
    public float[] LampO = new float[4]; //[4]
    public float[] LampM = new float[4]; //[4]
    public float[] Romb = new float[4]; //[4]

    public DWORD TextureName;

    public HUDTexelData(BaseScene pScene, DWORD name) : base(pScene, name)
    {
        TextureName = Hasher.HshString("hud");

        GunSight[0] = 72;
        GunSight[1] = 57;
        GunSight[2] = 111;
        GunSight[3] = 96;

        BigRing[0] = 191;
        BigRing[1] = 39;
        BigRing[2] = 254;
        BigRing[3] = 102;

        Empty[0] = 1;
        Empty[1] = 54;
        Empty[2] = 2;
        Empty[3] = 55;

        CompasScale[0] = 2;
        CompasScale[1] = 130;
        CompasScale[2] = 38;
        CompasScale[3] = 136;

        //// N
        //CompasAzimut[0, 0] = 215;
        //CompasAzimut[0, 1] = 0;
        //CompasAzimut[0, 2] = 220;
        //CompasAzimut[0, 3] = 7;
        ////030
        //CompasAzimut[1, 0] = 2; //    169
        //CompasAzimut[1, 1] = 155; //    132
        //CompasAzimut[1, 2] = 13; //    180
        //CompasAzimut[1, 3] = 160; //    137

        ////060
        //CompasAzimut[2, 0] = 2;
        //CompasAzimut[2, 1] = 162;
        //CompasAzimut[2, 2] = 13;
        //CompasAzimut[2, 3] = 167;
        ////E
        //CompasAzimut[3, 0] = 170;
        //CompasAzimut[3, 1] = 0;
        //CompasAzimut[3, 2] = 174;
        //CompasAzimut[3, 3] = 7;
        ////120
        //CompasAzimut[4, 0] = 2;
        //CompasAzimut[4, 1] = 169;
        //CompasAzimut[4, 2] = 13;
        //CompasAzimut[4, 3] = 174;
        ////150
        //CompasAzimut[5, 0] = 2;
        //CompasAzimut[5, 1] = 176;
        //CompasAzimut[5, 2] = 13;
        //CompasAzimut[5, 3] = 181;
        ////S
        //CompasAzimut[6, 0] = 241;
        //CompasAzimut[6, 1] = 0;
        //CompasAzimut[6, 2] = 245;
        //CompasAzimut[6, 3] = 7;
        ////210
        //CompasAzimut[7, 0] = 2;
        //CompasAzimut[7, 1] = 183;
        //CompasAzimut[7, 2] = 13;
        //CompasAzimut[7, 3] = 188;
        ////240
        //CompasAzimut[8, 0] = 2;
        //CompasAzimut[8, 1] = 190;
        //CompasAzimut[8, 2] = 13;
        //CompasAzimut[8, 3] = 195;
        ////W
        //CompasAzimut[9, 0] = 6;
        //CompasAzimut[9, 1] = 10;
        //CompasAzimut[9, 2] = 11;
        //CompasAzimut[9, 3] = 17;
        ////300
        //CompasAzimut[10, 0] = 2;
        //CompasAzimut[10, 1] = 197;
        //CompasAzimut[10, 2] = 13;
        //CompasAzimut[10, 3] = 202;
        ////330
        //CompasAzimut[11, 0] = 2;
        //CompasAzimut[11, 1] = 204;
        //CompasAzimut[11, 2] = 13;
        //CompasAzimut[11, 3] = 209;

        //Init CompasAzimut Jagger Array;
        for (int i=0;i< CompasAzimut.Length;i++)
        {
            CompasAzimut[i] = new float[4];
        }
        // N
        CompasAzimut[0][0] = 215;
        CompasAzimut[0][1] = 0;
        CompasAzimut[0][2] = 220;
        CompasAzimut[0][3] = 7;
        //030
        CompasAzimut[1][0] = 2; //    169
        CompasAzimut[1][1] = 155; //    132
        CompasAzimut[1][2] = 13; //    180
        CompasAzimut[1][3] = 160; //    137

        //060
        CompasAzimut[2][0] = 2;
        CompasAzimut[2][1] = 162;
        CompasAzimut[2][2] = 13;
        CompasAzimut[2][3] = 167;
        //E
        CompasAzimut[3][0] = 170;
        CompasAzimut[3][1] = 0;
        CompasAzimut[3][2] = 174;
        CompasAzimut[3][3] = 7;
        //120
        CompasAzimut[4][0] = 2;
        CompasAzimut[4][1] = 169;
        CompasAzimut[4][2] = 13;
        CompasAzimut[4][3] = 174;
        //150
        CompasAzimut[5][0] = 2;
        CompasAzimut[5][1] = 176;
        CompasAzimut[5][2] = 13;
        CompasAzimut[5][3] = 181;
        //S
        CompasAzimut[6][0] = 241;
        CompasAzimut[6][1] = 0;
        CompasAzimut[6][2] = 245;
        CompasAzimut[6][3] = 7;
        //210
        CompasAzimut[7][0] = 2;
        CompasAzimut[7][1] = 183;
        CompasAzimut[7][2] = 13;
        CompasAzimut[7][3] = 188;
        //240
        CompasAzimut[8][0] = 2;
        CompasAzimut[8][1] = 190;
        CompasAzimut[8][2] = 13;
        CompasAzimut[8][3] = 195;
        //W
        CompasAzimut[9][0] = 6;
        CompasAzimut[9][1] = 10;
        CompasAzimut[9][2] = 11;
        CompasAzimut[9][3] = 17;
        //300
        CompasAzimut[10][0] = 2;
        CompasAzimut[10][1] = 197;
        CompasAzimut[10][2] = 13;
        CompasAzimut[10][3] = 202;
        //330
        CompasAzimut[11][0] = 2;
        CompasAzimut[11][1] = 204;
        CompasAzimut[11][2] = 13;
        CompasAzimut[11][3] = 209;

        CompasBox[0] = 45;
        CompasBox[1] = 59;
        CompasBox[2] = 65;
        CompasBox[3] = 72;
        //    *----------*
        AviaHor[0] = 127;
        AviaHor[1] = 33;
        AviaHor[2] = 200;
        AviaHor[3] = 38;
        //*-------------|-------------*
        AviaHor1[0] = 78;
        AviaHor1[1] = 105;
        AviaHor1[2] = 227;
        AviaHor1[3] = 130;
        //*----------       ----------*
        AviaHor2[0] = 78;
        AviaHor2[1] = 132;
        AviaHor2[2] = 227;
        AviaHor2[3] = 135;
        // ^
        TargetUp[0] = 16;
        TargetUp[1] = 142;
        TargetUp[2] = 21;
        TargetUp[3] = 147;
        //V
        TargetDown[0] = 16;
        TargetDown[1] = 147;
        TargetDown[2] = 21;
        TargetDown[3] = 142;
        //<
        TargetLeft[0] = 29;
        TargetLeft[1] = 142;
        TargetLeft[2] = 24;
        TargetLeft[3] = 147;
        //>
        TargetRight[0] = 24;
        TargetRight[1] = 142;
        TargetRight[2] = 29;
        TargetRight[3] = 147;

        //{ 1, 130, 19, 138 },        // |-
        TargetNW[0] = 129;
        TargetNW[1] = 57;
        TargetNW[2] = 185;
        TargetNW[3] = 91;

        //{ 18, 130, 0, 138 },        // -|
        TargetNE[0] = 185;
        TargetNE[1] = 57;
        TargetNE[2] = 129;
        TargetNE[3] = 91;

        //{ 1, 137, 19, 129 },        // |_
        TargetSW[0] = 129;
        TargetSW[1] = 91;
        TargetSW[2] = 185;
        TargetSW[3] = 57;

        //{ 18, 137, 0, 129 }};       // _|
        TargetSE[0] = 185;
        TargetSE[1] = 91;
        TargetSE[2] = 129;
        TargetSE[3] = 57;

        LineLeft[0] = 188;
        LineLeft[1] = 44;
        LineLeft[2] = 183;
        LineLeft[3] = 47;

        LineRight[0] = 183;
        LineRight[1] = 44;
        LineRight[2] = 188;
        LineRight[3] = 47;

        LineTop[0] = 184;
        LineTop[1] = 50;
        LineTop[2] = 187;
        LineTop[3] = 55;

        LineBottom[0] = 184;
        LineBottom[1] = 55;
        LineBottom[2] = 187;
        LineBottom[3] = 50;

        LScale[0] = 45;
        LScale[1] = 74;
        LScale[2] = 51;
        LScale[3] = 94;

        RScale[0] = 61;
        RScale[1] = 74;
        RScale[2] = 69;
        RScale[3] = 94;

        HScale[0] = 53;
        HScale[1] = 74;
        HScale[2] = 59;
        HScale[3] = 94;

        OffScale[0] = 65;
        OffScale[1] = 30;
        OffScale[2] = 62;
        OffScale[3] = 37;

        Box[0] = 45;
        Box[1] = 44;
        Box[2] = 70;
        Box[3] = 57;

        LBracket[0] = 45;
        LBracket[1] = 59;
        LBracket[2] = 48;
        LBracket[3] = 72;

        RBracket[0] = 62;
        RBracket[1] = 59;
        RBracket[2] = 65;
        RBracket[3] = 72;

        Point[0] = 3;
        Point[1] = 142;
        Point[2] = 4;
        Point[3] = 143;

        PointBlack[0] = 3;
        PointBlack[1] = 148;
        PointBlack[2] = 4;
        PointBlack[3] = 149;

        Tank[0] = 154;
        Tank[1] = 218;
        Tank[2] = 170;
        Tank[3] = 255;

        Turet[0] = 171;
        Turet[1] = 218;
        Turet[2] = 187;
        Turet[3] = 255;

        Craft[0] = 188;
        Craft[1] = 218;
        Craft[2] = 204;
        Craft[3] = 255;

        SeaShip[0] = 205;
        SeaShip[1] = 218;
        SeaShip[2] = 221;
        SeaShip[3] = 255;

        AirShip[0] = 222;
        AirShip[1] = 218;
        AirShip[2] = 238;
        AirShip[3] = 255;

        Rocket[0] = 239;
        Rocket[1] = 218;
        Rocket[2] = 255;
        Rocket[3] = 255;

        Select[0] = 7;
        Select[1] = 141;
        Select[2] = 13;
        Select[3] = 152;

        //Bar[0, 0] = 18;
        //Bar[0, 1] = 44;
        //Bar[0, 2] = 32;
        //Bar[0, 3] = 123;

        //Bar[1, 0] = 34;
        //Bar[1, 1] = 44;
        //Bar[1, 2] = 43;
        //Bar[1, 3] = 123;

        //Bar[2, 0] = 1;  //0..25..50..75..100
        //Bar[2, 1] = 44;
        //Bar[2, 2] = 16;
        //Bar[2, 3] = 123;
        for (int i = 0; i < Bar.Length; i++)
        {
            Bar[i] = new float[4];
        }
        Bar[0][0] = 18;
        Bar[0][1] = 44;
        Bar[0][2] = 32;
        Bar[0][3] = 123;

        Bar[1][0] = 34;
        Bar[1][1] = 44;
        Bar[1][2] = 43;
        Bar[1][3] = 123;

        Bar[2][0] = 1;  //0..25..50..75..100
        Bar[2][1] = 44;
        Bar[2][2] = 16;
        Bar[2][3] = 123;

        BarCount[0] = 10;
        BarCount[1] = 20;
        BarCount[2] = 100;

        Bar100[0] = 1;  //0..25..50..75..100
        Bar100[1] = 44;
        Bar100[2] = 16;
        Bar100[3] = 123;

        WBar[0] = 30;
        WBar[1] = 30;
        WBar[2] = 65;
        WBar[3] = 37;

        WBox[0] = 72;
        WBox[1] = 44;
        WBox[2] = 179;
        WBox[3] = 55;

        TriangleFull[0] = 53;
        TriangleFull[1] = 97;
        TriangleFull[2] = 68;
        TriangleFull[3] = 113;

        Triangle[0] = 53;
        Triangle[1] = 115;
        Triangle[2] = 68;
        Triangle[3] = 131;

        LampA[0] = 2;
        LampA[1] = 216;
        LampA[2] = 20;
        LampA[3] = 234;

        LampT[0] = 21;
        LampT[1] = 216;
        LampT[2] = 39;
        LampT[3] = 234;

        LampO[0] = 40;
        LampO[1] = 216;
        LampO[2] = 58;
        LampO[3] = 234;

        LampM[0] = 59;
        LampM[1] = 216;
        LampM[2] = 77;
        LampM[3] = 234;

        Romb[0] = 33;
        Romb[1] = 153;
        Romb[2] = 74;
        Romb[3] = 195;

    }
};