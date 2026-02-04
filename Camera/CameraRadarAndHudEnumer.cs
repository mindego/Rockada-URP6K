using UnityEngine;
using static HudDataColors;
// radar and hud enumerator for CameraLogics
public class CameraRadarAndHudEnumer : CameraRadarEnumer
{
    private uint myHash;
    const float MarkMinHudRadius = .25f;
    // own
    protected HudRecticlesData mpRecticles;
    CameraData mrData;
    iContact mpSuggestedTarget;
    HudRecticleData mpLastData;
    float mHudRange2;
    protected override void AddRadarItem(iContact c, float r2, bool my)
    {
        AddHudItem(c, r2, my);
        base.AddRadarItem(c, r2, my);
    }
    protected virtual void AddHudItem(iContact c, float r2, bool my)
    {
        // проверяем контакт
        if (c.GetFpo() == null || c == mpSelf || c == mpTarget || c == mpSuggestedTarget || r2 > mHudRange2) return;
        // пытаемся его спроецировать
        RO_INFO r;
        bool b = false;
        if (RO.ProjectSphere(out r, mrData.myCamera, mrData.CameraAspect, c.GetOrg(), c.GetRadius()))
        {
            // проверяем на попадание в экран
            if (r.sr < MarkMinHudRadius)
            {
                b = (r.sx > -MarkMinHudRadius && r.sx < 1f + MarkMinHudRadius || r.sy > -MarkMinHudRadius && r.sy < 1f + MarkMinHudRadius);
            }
            else
            {
                b = (r.sx > -r.sr && r.sx < 1f + r.sr || r.sy > -r.sr && r.sy < 1f + r.sr);
            }
        }
        // если видно
        if (b == true)
        {
            // ищем следующий наш recticle
            mpLastData = (mpLastData != null ? mpLastData.Next() : mpRecticles.Items.Head());
            for (; mpLastData != null; mpLastData = mpLastData.Next())
            {
                if (mpLastData.UID == myHash) //TODO В это случае сравниваются указатели. НУ и вот как с этим бороться? 
                    break;
            }
            // если не нашли, создаем
            if (mpLastData == null) mpLastData = mpRecticles.AddItem(HudRecticleData.TYPE.SIMPLETARGET, 0, 0, 0, myHash);
            // ставим данные
            mpLastData.x = r.sx;
            mpLastData.y = r.sy;
            mpLastData.setRadius(Mathf.Clamp(r.sr * .5f, 0, CameraDataCockpit.HudDeviceMax));
            mpLastData.hide = false;
            PrintContactInfo(ref mpLastData, c, my);
        }
    }
    BaseScene myScene;
    // api
    public bool mShowFriendlyName;
    public bool mShowFriendlyType;
    public bool mShowFriendlyDist;
    public bool mShowEnemyName;
    public bool mShowEnemyType;
    public bool mShowEnemyDist;
    public CameraRadarAndHudEnumer(BaseScene s, HudRadarData p, HudRecticlesData r, CameraData d) : base(p)
    {
        //mindego
        myHash = (uint)GetHashCode();

        mpRecticles = r;
        mrData = d;
        myScene = s;
    }
    ~CameraRadarAndHudEnumer()
    {
        // удаляем все наши значки
        HudRecticleData pNext;
        for (mpLastData = mpRecticles.Items.Head(); mpLastData != null; mpLastData = pNext)
        {
            pNext = mpLastData.Next();
            if (mpLastData.UID != myHash) //TODO В это случае сравниваются указатели. НУ и вот как с этим бороться? 
                continue;
            mpRecticles.DeleteItem(mpLastData);
        }
    }
    public void Enumerate(iContact self, iContact tgt, iContact stgt, iSensors s, float look_back = 0)
    {
        // готовимся
        mpSuggestedTarget = stgt;
        mpLastData = null;
        // обрабатываем цели
        mHudRange2 = Mathf.Pow(mpRadar.range, 2);
        base.Enumerate(self, tgt, s, look_back);
        // прячем все наши значки, которым не досталось цели
        mpLastData = (mpLastData != null ? mpLastData.Next() : mpRecticles.Items.Head());
        for (; mpLastData != null; mpLastData = mpLastData.Next())
        {
            if (mpLastData.UID == myHash)
                mpLastData.hide = true;
        }
    }
    public void Hide()
    {
        // прячем все наши значки
        for (mpLastData = mpRecticles.Items.Head(); mpLastData != null; mpLastData = mpLastData.Next())
        {
            if (mpLastData.UID == myHash)
                mpLastData.hide = true;
        }
    }
    public void PrintContactInfo(ref HudRecticleData pData, iContact pContact, bool IsMy)
    {
        // ставим цвет
        bool DrawType;
        bool DrawName;
        bool DrawDist;
        if (IsMy == true)
        {
            pData.colour = HUDCOLOR_HUMAN;
            DrawType = mShowFriendlyType;
            DrawName = mShowFriendlyName;
            DrawDist = mShowFriendlyDist;
        }
        else
        {
            pData.colour = (uint)(pContact.GetSideCode() == 0 ? HUDCOLOR_NEUTRAL : HUDCOLOR_VELIAN);
            DrawType = mShowEnemyType;
            DrawName = mShowEnemyName;
            DrawDist = mShowEnemyDist;
        }
        // пишем текст
        if (DrawName == true)
        {
            //            char buffer[128];
            string buffer = "";
            bool ret = myScene.AddLocalizedString(ref buffer, 128, 0, pContact.GetName(), false, 0) !=null;
            /*if (ret)
                dprintf("input: %s, output: %s\n",pContact->GetName(),buffer);
            else
                dprintf("input: %s - skipped \n",pContact->GetName());*/
            //StrCpy(pData->textup, ret ? buffer : pContact->GetName());
            pData.textup = ret ? buffer : pContact.GetName();
        }
        else pData.textup = null;
        if (DrawType == true && DrawDist == true)
        {
            pData.text = string.Format("{0}: {1:F2}km", pContact.GetTypeName(), (pContact.GetOrg() - mrData.myCamera.Org).magnitude * .001f);
            //Sprintf(pData->text, "%s: %.1fkm", pContact->GetTypeName(), (pContact->GetOrg() - mrData.Camera.Org).Norma() * .001f);
        }
        else
        {
            pData.text = null;
            if (DrawType == true) pData.text = pContact.GetTypeName();
            if (DrawDist == true) pData.text = string.Format("{0:F2}km", (pContact.GetOrg() - mrData.myCamera.Org).magnitude * .001f);
        }
    }
};

