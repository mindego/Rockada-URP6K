using UnityEngine;
using static HudDataColors;
using DWORD = System.UInt32;
using ExtensionMethods;

public class HudColors : BaseData, CommLink
{
    public Color32[] colors = new Color32[HUDCOLOR_MAX];
    public Color32 human;
    public Color32 velian;
    public Color32 neutral;
    public Color32 normal;
    public Color32 medium;
    public Color32 danger;
    public Color32 disabled;
    public Color32 radar;
    public Color32 selected;
    public Color32 background;
    public Color32 text;
    public Color32 map;
    public Color32 header_back;
    public Color32 header_text;
    public Color32 plain_back;
    public Color32 plain_text;
    public Color32 select_back;
    public Color32 select_text;
    public Color32 team1_back;
    public Color32 team1_text;
    public Color32 team2_back;
    public Color32 team2_text;
    public Color32 team3_back;
    public Color32 team3_text;
    public Color32 team4_back;
    public Color32 team4_text;

    public Color32 custom0;
    public Color32 custom1;
    public Color32 custom2;
    public Color32 custom3;
    public Color32 custom4;
    public Color32 custom5;
    public Color32 custom6;
    public Color32 custom7;
    public Color32 custom8;
    public Color32 custom9;
    public Color32 horizon;
    public Color32 gunsign;
    public Color32 mouse;
    public Color32 mouse_out;

    int var_velian;
    int var_human;
    int var_neutral;
    int var_normal;
    int var_medium;
    int var_danger;
    int var_disabled;
    int var_radar;
    int cmd_init;
    int var_alpha;
    int var_selected;
    int var_background;
    int var_text;
    int var_map;
    int var_header_back;
    int var_header_text;
    int var_plain_back;
    int var_plain_text;
    int var_select_back;
    int var_select_text;
    int var_team1_back;
    int var_team1_text;
    int var_team2_back;
    int var_team2_text;
    int var_team3_back;
    int var_team3_text;
    int var_team4_back;
    int var_team4_text;
    int var_custom0;
    int var_custom1;
    int var_custom2;
    int var_custom3;
    int var_custom4;
    int var_custom5;
    int var_custom6;
    int var_custom7;
    int var_custom8;
    int var_custom9;
    int var_horizon;
    int var_gunsign;
    int var_mouse;
    int var_mouse_out;

    bool changed;
    public HudColors(BaseScene pScene, DWORD iname, string sname) : base(pScene, iname)
    {
        //colors[HUDCOLOR_HUMAN] = human;
        //colors[HUDCOLOR_VELIAN] = velian;
        //colors[HUDCOLOR_NEUTRAL] = neutral;
        //colors[HUDCOLOR_NORMAL] = normal;
        //colors[HUDCOLOR_MEDIUM] = medium;
        //colors[HUDCOLOR_DANGER] = danger;
        //colors[HUDCOLOR_DISABLED] = disabled;
        //colors[HUDCOLOR_RADAR] = radar;
        //colors[HUDCOLOR_SELECTED] = selected;
        //colors[HUDCOLOR_BACKGROUND] = background;
        //colors[HUDCOLOR_TEXT] = text;
        //colors[HUDCOLOR_MAP] = map;
        //colors[HUDCOLOR_HEADER_BACK] = header_back;
        //colors[HUDCOLOR_HEADER_TEXT] = header_text;
        //colors[HUDCOLOR_PLAIN_BACK] = plain_back;
        //colors[HUDCOLOR_PLAIN_TEXT] = plain_text;
        //colors[HUDCOLOR_SELECT_BACK] = select_back;
        //colors[HUDCOLOR_SELECT_TEXT] = select_text;
        //colors[HUDCOLOR_TEAM1_BACK] = team1_back;
        //colors[HUDCOLOR_TEAM1_TEXT] = team1_text;
        //colors[HUDCOLOR_TEAM2_BACK] = team2_back;
        //colors[HUDCOLOR_TEAM2_TEXT] = team2_text;
        //colors[HUDCOLOR_TEAM3_BACK] = team3_back;
        //colors[HUDCOLOR_TEAM3_TEXT] = team3_text;
        //colors[HUDCOLOR_TEAM4_BACK] = team4_back;
        //colors[HUDCOLOR_TEAM4_TEXT] = team4_text;

        //colors[HUDCOLOR_HORIZON] = horizon;
        //colors[HUDCOLOR_GUNSIGN] = gunsign;
        //colors[HUDCOLOR_MOUSE] = mouse;
        //colors[HUDCOLOR_MOUSE_OUT] = mouse_out;
        //colors[HUDCOLOR_CUSTOM8] = custom8;
        //colors[HUDCOLOR_CUSTOM9] = custom9;
        //colors[HUDCOLOR_CUSTOM0] = custom0;
        //colors[HUDCOLOR_CUSTOM1] = custom1;
        //colors[HUDCOLOR_CUSTOM2] = custom2;
        //colors[HUDCOLOR_CUSTOM3] = custom3;
        //colors[HUDCOLOR_CUSTOM4] = custom4;
        //colors[HUDCOLOR_CUSTOM5] = custom5;
        //colors[HUDCOLOR_CUSTOM6] = custom6;
        //colors[HUDCOLOR_CUSTOM7] = custom7;

        //HUD_REGISTER_VAR(human, VAR_SVEC4);
        //HUD_REGISTER_VAR(velian, VAR_SVEC4);
        //HUD_REGISTER_VAR(neutral, VAR_SVEC4);
        //HUD_REGISTER_VAR(normal, VAR_SVEC4);
        //HUD_REGISTER_VAR(medium, VAR_SVEC4);
        //HUD_REGISTER_VAR(danger, VAR_SVEC4);
        //HUD_REGISTER_VAR(disabled, VAR_SVEC4);
        //HUD_REGISTER_VAR(radar, VAR_SVEC4);
        //HUD_REGISTER_VAR(selected, VAR_SVEC4);
        //HUD_REGISTER_VAR(background, VAR_SVEC4);
        //HUD_REGISTER_VAR(text, VAR_SVEC4);
        //HUD_REGISTER_VAR(map, VAR_SVEC4);
        //HUD_REGISTER_VAR(header_back, VAR_SVEC4);
        //HUD_REGISTER_VAR(header_text, VAR_SVEC4);
        //HUD_REGISTER_VAR(plain_back, VAR_SVEC4);
        //HUD_REGISTER_VAR(plain_text, VAR_SVEC4);
        //HUD_REGISTER_VAR(select_back, VAR_SVEC4);
        //HUD_REGISTER_VAR(select_text, VAR_SVEC4);
        //HUD_REGISTER_VAR(team1_back, VAR_SVEC4);
        //HUD_REGISTER_VAR(team1_text, VAR_SVEC4);
        //HUD_REGISTER_VAR(team2_back, VAR_SVEC4);
        //HUD_REGISTER_VAR(team2_text, VAR_SVEC4);
        //HUD_REGISTER_VAR(team3_back, VAR_SVEC4);
        //HUD_REGISTER_VAR(team3_text, VAR_SVEC4);
        //HUD_REGISTER_VAR(team4_back, VAR_SVEC4);
        //HUD_REGISTER_VAR(team4_text, VAR_SVEC4);

        //HUD_REGISTER_VAR(horizon, VAR_SVEC4);
        //HUD_REGISTER_VAR(gunsign, VAR_SVEC4);
        //HUD_REGISTER_VAR(mouse, VAR_SVEC4);
        //HUD_REGISTER_VAR(mouse_out, VAR_SVEC4);
        //HUD_REGISTER_VAR(custom8, VAR_SVEC4);
        //HUD_REGISTER_VAR(custom9, VAR_SVEC4);
        //HUD_REGISTER_VAR(custom0, VAR_SVEC4);
        //HUD_REGISTER_VAR(custom1, VAR_SVEC4);
        //HUD_REGISTER_VAR(custom2, VAR_SVEC4);
        //HUD_REGISTER_VAR(custom3, VAR_SVEC4);
        //HUD_REGISTER_VAR(custom4, VAR_SVEC4);
        //HUD_REGISTER_VAR(custom5, VAR_SVEC4);
        //HUD_REGISTER_VAR(custom6, VAR_SVEC4);
        //HUD_REGISTER_VAR(custom7, VAR_SVEC4);

        //HUD_REGISTER_CMD(init, 0);
        Init();
    }
    ~HudColors()
    {
        rScene.GetCommandsApi().UnRegister(this);
    }

    private void SetColorArray()
    {
        //Вынесено в отдельный метод, т.к в коде шторма это указатели
        colors[HUDCOLOR_HUMAN] = human;
        colors[HUDCOLOR_VELIAN] = velian;
        colors[HUDCOLOR_NEUTRAL] = neutral;
        colors[HUDCOLOR_NORMAL] = normal;
        colors[HUDCOLOR_MEDIUM] = medium;
        colors[HUDCOLOR_DANGER] = danger;
        colors[HUDCOLOR_DISABLED] = disabled;
        colors[HUDCOLOR_RADAR] = radar;
        colors[HUDCOLOR_SELECTED] = selected;
        colors[HUDCOLOR_BACKGROUND] = background;
        colors[HUDCOLOR_TEXT] = text;
        colors[HUDCOLOR_MAP] = map;
        colors[HUDCOLOR_HEADER_BACK] = header_back;
        colors[HUDCOLOR_HEADER_TEXT] = header_text;
        colors[HUDCOLOR_PLAIN_BACK] = plain_back;
        colors[HUDCOLOR_PLAIN_TEXT] = plain_text;
        colors[HUDCOLOR_SELECT_BACK] = select_back;
        colors[HUDCOLOR_SELECT_TEXT] = select_text;
        colors[HUDCOLOR_TEAM1_BACK] = team1_back;
        colors[HUDCOLOR_TEAM1_TEXT] = team1_text;
        colors[HUDCOLOR_TEAM2_BACK] = team2_back;
        colors[HUDCOLOR_TEAM2_TEXT] = team2_text;
        colors[HUDCOLOR_TEAM3_BACK] = team3_back;
        colors[HUDCOLOR_TEAM3_TEXT] = team3_text;
        colors[HUDCOLOR_TEAM4_BACK] = team4_back;
        colors[HUDCOLOR_TEAM4_TEXT] = team4_text;

        colors[HUDCOLOR_HORIZON] = horizon;
        colors[HUDCOLOR_GUNSIGN] = gunsign;
        colors[HUDCOLOR_MOUSE] = mouse;
        colors[HUDCOLOR_MOUSE_OUT] = mouse_out;
        colors[HUDCOLOR_CUSTOM8] = custom8;
        colors[HUDCOLOR_CUSTOM9] = custom9;
        colors[HUDCOLOR_CUSTOM0] = custom0;
        colors[HUDCOLOR_CUSTOM1] = custom1;
        colors[HUDCOLOR_CUSTOM2] = custom2;
        colors[HUDCOLOR_CUSTOM3] = custom3;
        colors[HUDCOLOR_CUSTOM4] = custom4;
        colors[HUDCOLOR_CUSTOM5] = custom5;
        colors[HUDCOLOR_CUSTOM6] = custom6;
        colors[HUDCOLOR_CUSTOM7] = custom7;
    }
    void Init()
    {
        //rgba
        human.Set(100, 160, 0, 128);

        neutral.Set(0, 0, 255, 128);
        velian.Set(255, 0, 0, 128);
        normal.Set(100, 160, 0, 128);
        medium.Set(200, 200, 0, 128);
        danger.Set(255, 0, 0, 128);
        disabled.Set(100, 100, 100, 128);
        radar.Set(255, 255, 255, 128);
        selected.Set(255, 255, 255, 128);
        background.Set(0, 0, 0, 128);
        text.Set(100, 160, 0, 255);
        map.Set(255, 255, 255, 255);

        header_back.Set(125, 125, 125, 230);//> header_back --цвет фасетки заголовка        230 125 125 125        
        header_text.Set(100, 220, 230, 255);//> header_text --цвет текста заголовка         255 100 220 230         
        plain_back.Set(5, 10, 20, 40);//> plain_back  --цвет фасетки обычной строки    40   5  10  20         
        plain_text.Set(220, 220, 220, 255);//> plain_text  --цвет текста обычной строки    150  60  20  10         
        select_back.Set(30, 5, 5, 30);//> select_back --цвет фасетки выделенной строки 30  30   5   5      
        select_text.Set(230, 150, 10, 255);//> select_text --цвет текста выделенной строки 255 230 150  10 

        team1_back.Set(60, 0, 0, 70);//> team1_back   70 60  0  0 
        team1_text.Set(0, 0, 0, 120);//> team1_text  120  0  0  0 (рекомендуется)   
        team2_back.Set(0, 40, 40, 110);//> team2_back  110  0 40 40 
        team2_text.Set(0, 0, 0, 120);//> team2_text  120  0  0  0 
        team3_back.Set(20, 70, 20, 120);//> team3_back  120 20 70 20 
        team3_text.Set(0, 0, 0, 120);//> team3_text  120  0  0  0 
        team4_back.Set(30, 30, 0, 70);//> team4_back   70 30 30  0 
        team4_text.Set(0, 0, 0, 120);//> team4_text  120  0  0  0 

        horizon.Set(100, 160, 0, 128);
        gunsign.Set(100, 160, 0, 128);
        mouse.Set(100, 160, 0, 128);
        mouse_out.Set(255, 0, 0, 128);
        custom8.Set(100, 160, 0, 128);
        custom9.Set(100, 160, 0, 128);
        custom0.Set(100, 160, 0, 128);
        custom1.Set(100, 160, 0, 128);
        custom2.Set(100, 160, 0, 128);
        custom3.Set(100, 160, 0, 128);
        custom4.Set(100, 160, 0, 128);
        custom5.Set(100, 160, 0, 128);
        custom6.Set(100, 160, 0, 128);
        custom7.Set(100, 160, 0, 128);
        changed = true;
        SetColorArray();
    }

    public virtual object OnVariable(int code ,object data)
    {
        //TODO Реализовать установку цвета по подписи в устройстве HUDа
        return null;
    }
    public void OnCommand(int code, string a, string b)
    {
        if (code == cmd_init) Init();
    }
};

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static void Set(this ref Color32 color, int r, int g, int b, int a)
        {
            color.r = (byte)r;
            color.g = (byte)g;
            color.b = (byte)b;
            color.a = (byte)a;
        }
    }
}