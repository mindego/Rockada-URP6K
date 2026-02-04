using System.Collections.Generic;
using System.IO;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using static MenuMusix;
using static Unity.Burst.Intrinsics.X86.Avx;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

public class DlgMissionResult
{
    public override string ToString()
    {
        return string.Format("Craft: {0}\nMission: {1}\nDB: {2}",CratftName.ToString("X8"),MissionName,DbName);
    }

    public DWORD CratftName;
    public DWORD[] Weapon = new DWORD[3];
    //char MissionName[128];
    //char DbName[128];
    public string MissionName;
    public string DbName;

}
public class CbWeaponEnumer : IEnumer<EnumItemInfo>
{
    UnityMenuDevCombobox myCb;
    public CbWeaponEnumer(UnityMenuDevCombobox cb)
    {
        myCb = cb;
    }

    public void process(EnumItemInfo str)
    {
        if (myCb.findString(str.myFullName) == UnityMenuDevCombobox.badIndex)
            myCb.AddStringLP(str.myFullName, str.myName);
    }
};
public class MenuDevUnity
{
    const int COOSEMSN_VER = 0x00000002;
    public const int IDD_DIALOG = 101;
    public const int IDC_COMBO1 = 1000;
    public const int IDC_COMBO2 = 1001;
    public const int IDC_WP1 = 1003;
    public const int IDC_WP2 = 1004;
    public const int IDC_WP3 = 1005;
    public const int IDC_CRAFT = 1006;

    public static int chooseMission(out DlgMissionResult r, string DataPath, int Version = COOSEMSN_VER)
    {
        r = new DlgMissionResult();
        if (Version != COOSEMSN_VER) return 0;

        GameObject MenuDevUnityStorage = GameObject.Find("MenuWin");
        if (MenuDevUnityStorage == null) return 0;
        UIDocument myUIDocument = MenuDevUnityStorage.GetComponent<UIDocument>();
        if (myUIDocument == null) return 0;
        return new DialogWinUnity(r, DataPath, myUIDocument).display();
    }

    public static int chooseMission(string DataPath, MainMenuWin2Unity.GameOpener opener, int Version = COOSEMSN_VER)
    {
        DlgMissionResult r = new DlgMissionResult();
        if (Version != COOSEMSN_VER) return 0;

        GameObject MenuDevUnityStorage = GameObject.Find("MenuWin");
        if (MenuDevUnityStorage == null) return 0;
        UIDocument myUIDocument = MenuDevUnityStorage.GetComponent<UIDocument>();
        if (myUIDocument == null) return 0;
        return new DialogWinUnity(r, DataPath, myUIDocument).display(opener);
    }

    public static iUnifiedVariableDB openSelectedUdb(UnityMenuDevCombobox cb, string path)
    {
        string file = path;
        cb.GetSelString(ref file);
        Debug.Log("Opening file: " + (path + file));
        return UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, path+file, true);
    }
}

public class DialogWinUnity : IDialog
{
    DlgMissionResult myResult;
    string myPath;
    UIDocument myDialog;
    private Dictionary<string, UnityMenuDevCombobox> comboBoxCache = new Dictionary<string, UnityMenuDevCombobox>();



    public DialogWinUnity(DlgMissionResult r, string path, UIDocument DialogUI)
    {
        myResult = r;
        myPath = path;
        myDialog = DialogUI;

    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    private UnityMenuDevCombobox GetComboBox(string name)
    {
        if (!comboBoxCache.ContainsKey(name)) comboBoxCache.Add(name, new UnityMenuDevCombobox(this,myDialog, name));

        return comboBoxCache[name];
    }

    #region плеер музыки
    //в оригинале в Win меню музыки нет. Реализовано чисто для прикола
    MenuMusix myMusic;
    ISound mySound;
    void startMusic() { myMusic.play("Menu.wav", mySound, MainMenuStorm_dll.openMusic); }
    void stopMusic() { myMusic.stop(); }
    public void PlayMenuMusic()
    {
        if (myMusic == null) myMusic=new MenuMusix();
        if (mySound == null) {
            SoundConfig cfg  = new SoundConfig(true);
            Sound tmpSound = new Sound();
            tmpSound.Initialize(null, cfg);
            
            SoundApi myISound = new SoundApi();
            myISound.Initialize(tmpSound,cfg, null, null);
            mySound = myISound;
        }
        startMusic();
    }
    public void StopMenuMusic()
    {
        if (myMusic == null) return;
        stopMusic();
    }
    #endregion
    public virtual bool OnInit()
    {
        PlayMenuMusic();
        UnityMenuDevButton btn = new UnityMenuDevButton(this,myDialog,"IDOK");
        UnityMenuDevCombobox sets = GetComboBox("IDC_COMBO1");
        FindFile ff = new FindFile(myPath, "*.gsd");
        if (ff.HasFile()) do
                if (!ff.IsDirectory())
                {
                    sets.AddStringLP(ff.GetFileName());
                }
            while (ff.Next());

        if (sets.GetCount() != 0)
        {
            DWORD idx=0;
            setLatestIndexes("Index1",ref idx);
            sets.SetCurSelection(0);
            if (idx < sets.GetCount())
                sets.SetCurSelection(idx);
            UpdateCampain(true);
            if (setGameData())
                return true;
        }

        EndDialog(false);
        //return false;
        return false;
    }
    void OnOk()
    {
        StopMenuMusic();
        UnityMenuDevCombobox sets = GetComboBox("IDC_COMBO1");
        UnityMenuDevCombobox msnlist = GetComboBox("IDC_COMBO2");

        bool ok = sets.GetSelString(ref myResult.DbName) &&
            msnlist.GetSelString(ref myResult.MissionName);

        //if (ok) *__strrchr(myResult.DbName, '.') = 0;

        storeLatestIndexes();

        //Debug.Log("myResult " + myResult);
        EndDialog(ok);
    }
    public void EndDialog(bool ok)
    {
        myDialog.enabled = false;
        //Debug.Log("myResult " + myResult);
        if (myOpener != null) myOpener(myResult);
    }

    public bool OnCommand(int Code, int idCtrl, string hCtrl)
    {
        switch (hCtrl)
        {
            case "IDOK":
                OnOk();
                break;

            case "IDCANCEL":
                EndDialog(false); break;

            case "IDC_COMBO1":
                if (Code == UnityMenuDevCombobox.CBN_SELCHANGE)
                    UpdateCampain();
                break;

            case "IDC_CRAFT":
                if (Code == UnityMenuDevCombobox.CBN_SELCHANGE)
                    initWeapons(GetComboBox(hCtrl).GetSelection<CRAFT_DATA>());
                break;

            case "IDC_WP1":
                if (Code == UnityMenuDevCombobox.CBN_SELCHANGE)
                    //                    readWpnComboResult(hCtrl, idCtrl - IDC_WP1);
                    readWpnComboResult(hCtrl, 0);
                break;
            case "IDC_WP2":
                if (Code == UnityMenuDevCombobox.CBN_SELCHANGE)
                    //                    readWpnComboResult(hCtrl, idCtrl - IDC_WP1);
                    readWpnComboResult(hCtrl, 1);
                break;
            case "IDC_WP3":
                //Assert.IsTrue((IDC_WP1 + 1) == IDC_WP2 && (IDC_WP2 + 1) == IDC_WP3);
                if (Code == UnityMenuDevCombobox.CBN_SELCHANGE)
                    //                    readWpnComboResult(hCtrl, idCtrl - IDC_WP1);
                    readWpnComboResult(hCtrl, 2);
                break;
        }

        return false;
    }

    void initWeapons(CRAFT_DATA cd)
    {

        myResult.CratftName = cd.Name;
        UnityMenuDevCombobox[] wp = new UnityMenuDevCombobox[] { GetComboBox("IDC_WP1"), GetComboBox("IDC_WP2"), GetComboBox("IDC_WP3") };
        string[] defWpnNames = new string[] { "PPC", "AC", "GM" };

        for (int i = 0; i < 3; ++i)
        {
            wp[i].Reset();
            //wp[i].AddString<crc32>(">> None <<", Storm.CRC32.CRC_NULL);
            wp[i].AddStringLP(">> None <<", CRC32.CRC_NULL);
            // EnableWindow(wp[i].m_hWnd, false);

            //LayoutEnumerator.enumWeapons<WPN_DATA>(cd, SUBOBJ_DATA.Datas, i, new CbWeaponEnumer(wp[i]));
            LayoutEnumerator.enumWeapons(cd, SUBOBJ_DATA.Datas, i, new CbWeaponEnumer(wp[i]));

            wp[i].SetCurSelection(defWpnNames[i]);
            readWpnComboResult(wp[i].name, i);
            //EnableWindow(wp[i].m_hWnd, wp[i].GetCount() > 1);
            // EnableWindow(wp[i].m_hWnd, false);
        }
    }

    void readWpnComboResult(string hCtrl, int wpIndex)
    {
        myResult.Weapon[wpIndex] = GetComboBox(hCtrl).GetSelection<crc32>();
    }
    bool setGameData()
    {
        UnityMenuDevCombobox crafts = GetComboBox("IDC_CRAFT");
        //for (OBJECT_DATA d = OBJECT_DATA.GetFirstItem(); d!=null; d = d.Next())
        foreach (OBJECT_DATA d in OBJECT_DATA.Datas)
            if (d.GetClass() == OBJECT_DATA.OC_CRAFT && d.Side == 0)
                crafts.AddString<OBJECT_DATA>(d.FullName, d);

        if (crafts.GetCount() != 0)
        {
            crafts.SetCurSelection("Human_BF4");
            initWeapons(crafts.GetSelection<CRAFT_DATA>());
            // EnableWindow(crafts.m_hWnd, false);
            return true;
        }
        else
        {
            errorBox("No one numan side craft found");
            return false;
        }
    }

    private void errorBox(string msg)
    {
        Debug.Log(msg);
    }
    void UpdateCampain(bool start = false)
    {
        UnityMenuDevCombobox msnlist = GetComboBox("IDC_COMBO2");
        msnlist.Reset();


        iUnifiedVariableDB db = MenuDevUnity.openSelectedUdb(GetComboBox("IDC_COMBO1"), myPath);
        if (db != null)
        {
            iUnifiedVariableContainer root = db.GetRootTpl<iUnifiedVariableContainer>();
            //Debug.Log("root " + root);
            if (root != null)
            {
                iUnifiedVariableContainer missions =
                    root.GetVariableTpl<iUnifiedVariableContainer>("Events");
                if (missions != null)
                {
                    for (DWORD h = 0; (h = missions.GetNextHandle(h)) != 0;)
                    {
                        iUnifiedVariableContainer msn =
                            missions.GetVariableTpl<iUnifiedVariableContainer>(h);

                        if (msn != null)
                        {
                            iUnifiedVariableInt type =
                                msn.GetVariableTpl<iUnifiedVariableInt>("Type");
                            //Debug.Log("type " + type);
                            if (type != null && (uint)type.GetValue()==0xA025345F)
                            {
                                string name = null;
                                msnlist.AddStringLP(missions.GetNameByHandle(ref name, h));
                                Debug.Log("Mission " + name + " added");
                            }
                        }
                    }
                }
            }
        }

        if (msnlist.GetCount() != 0)
        {
            msnlist.SetCurSelection(0);
            if (start)
            {
                DWORD idx = 0;
                setLatestIndexes("Index2", ref idx);
                if (idx < msnlist.GetCount())
                    msnlist.SetCurSelection(idx);
            }
        }

        //EnableWindow(GetDlgItem(hDlg, IDOK), msnlist.GetCount());
    }

    private string GetPrefixed(string s)
    {
        return "Software\\MADia\\MenuWin\\" + s;
    }
    private void setLatestIndexes(string key, ref DWORD result)
    {
        //HKEY hkey;
        //result = 0;
        //DWORD type = REG_DWORD, size = sizeof(DWORD);
        //RegOpenKeyEx(HKEY_CURRENT_USER, "Software\\MADia\\MenuWin", 0, KEY_QUERY_VALUE, &hkey);
        //RegQueryValueEx(hkey, key, NULL, &type, (LPBYTE) & result, &size);
        //RegCloseKey(hkey);

        //if (!PlayerPrefs.HasKey(ECHELON_PATH))
        //{
        //    PlayerPrefs.SetString(ECHELON_PATH, Path.GetDirectoryName(NativeDialog.GetEchelonPath()));
        //    PlayerPrefs.Save();
        //}
        if (PlayerPrefs.HasKey(GetPrefixed(key)))
        {
            result = (uint)PlayerPrefs.GetInt(GetPrefixed(key));
            return;
        }
    }

    void storeLatestIndexes()
    {
        UnityMenuDevCombobox sets = GetComboBox("IDC_COMBO1");
        UnityMenuDevCombobox msnlist = GetComboBox("IDC_COMBO2");

        int index1 =  sets.getSellectionIndex();
        int index2 =  msnlist.getSellectionIndex();

        PlayerPrefs.SetInt(GetPrefixed("Index1"), index1);
        PlayerPrefs.SetInt(GetPrefixed("Index2"), index2);
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    public int display()
    {
        OnInit();
        //STUB!
        return 0;
    }

    MainMenuWin2Unity.GameOpener myOpener;
    public int display(MainMenuWin2Unity.GameOpener opener )
    {
        myOpener = opener;
        OnInit();
        //STUB!
        return 0;
    }
}

/// <summary>
/// TODO - Перенести в впсмомогательные классы
/// </summary>
public class FindFile
{
    public FindFile(string myPath, string Mask = null, bool need_slash = false)
    {
        //if (need_slash) myPath += '\\';
        //if (Mask != null) myPath += Mask;

        string[] files = Mask != null ? Directory.GetFiles(myPath, Mask) : Directory.GetFiles(myPath);
        mfData = new WIN32_FIND_DATA[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            mfData[i] = new WIN32_FIND_DATA();
            var fInfo = new FileInfo(files[i]);
            //mfData[i].cFileName = Path.GetFileName(files[i]);
            mfData[i].cFileName = fInfo.Name;
            mfData[i].nFileSizeLow = (DWORD)fInfo.Length;
            mfData[i].dwFileAttributes = File.GetAttributes(files[i]);

        }
    }

    ~FindFile()
    {
        //if (mhFind != INVALID_HANDLE_VALUE)
        //    FindClose(mhFind);
    }

    //bool operator ++() { return FindNextFile(mhFind, &mfData); }
    //bool operator () ()         { return mhFind != INVALID_HANDLE_VALUE; }
    public bool Next()
    {
        return ++currentIndex < mfData.Length;

    }
    public bool HasFile()
    {
        return mfData.Length > 0;
    }

    public string GetFileName() { return mfData[currentIndex].cFileName; }
    public DWORD GetFileSize() { return mfData[currentIndex].nFileSizeLow; }
    public FileAttributes GetAttributes() { return mfData[currentIndex].dwFileAttributes; }
    public bool IsDirectory() { return GetAttributes().HasFlag(FileAttributes.Directory); }

    //HANDLE mhFind;
    WIN32_FIND_DATA[] mfData;
    int currentIndex = 0;
};

public class WIN32_FIND_DATA
{
    public string cFileName;
    public DWORD nFileSizeLow;
    public FileAttributes dwFileAttributes;
}

//public class MenuWin
//{
//    const int COOSEMSN_VER = 0x00000002;
//    public const int IDD_DIALOG = 101;
//    public const int IDC_COMBO1 = 1000;
//    public const int IDC_COMBO2 = 1001;
//    public const int IDC_WP1 = 1003;
//    public const int IDC_WP2 = 1004;
//    public const int IDC_WP3 = 1005;
//    public const int IDC_CRAFT = 1006;
//    public static iUnifiedVariableDB openSelectedUdb(ComboBox cb, string path)
//    {
//        string file = path;
//        cb.GetSelString(ref file);
//        return UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, file, true);
//    }

//    public static int chooseMission(out DlgMissionResult r, string DataPath, int Version = COOSEMSN_VER)
//    {
//        r = new DlgMissionResult();
//        if (Version != COOSEMSN_VER) return 0;
//        //return new DialogWin(out r, DataPath).display();
//        return 0;
//    }
//}

//public class DialogWin : IDialog
//{
//    DlgMissionResult myResult;
//    string myPath;

//    public DialogWin(DlgMissionResult r, string path) :{
//        myResult = r;
//        myPath = path;
//    }

//    virtual int GetResourceID() { return IDD_DIALOG; }

//    public virtual bool OnInit()
//    {
//        ComboBox sets = new ComboBox(MenuWin.IDC_COMBO1);
//        FindFile ff = new FindFile(myPath, "*.gsd");
//        if (ff()) do
//                if (!ff.IsDirectory())
//                {
//                    sets.AddStringLP(ff.GetFileName());
//                }
//            while (++ff);

//        if (sets.GetCount()!=0)
//        {
//            DWORD idx;
//            setLatestIndexes(hDlg, "Index1", idx);
//            sets.SetCurSelection(0);
//            if (idx < sets.GetCount())
//                sets.SetCurSelection(idx);
//            UpdateCampain(hDlg, true);
//            if (setGameData(hDlg))
//                return true;
//        }

//        EndDialog(hDlg, false);
//        return false;
//    }

//    void setLatestIndexes(string key, out DWORD result)
//    {
//        result = 0;
//        //HKEY hkey;
//        //result = 0;
//        //DWORD type = REG_DWORD, size = sizeof(DWORD);
//        //RegOpenKeyEx(HKEY_CURRENT_USER, "Software\\MADia\\MenuWin", 0, KEY_QUERY_VALUE, &hkey);
//        //RegQueryValueEx(hkey, key, NULL, &type, (LPBYTE) & result, &size);
//        //RegCloseKey(hkey);
//    }

//    void storeLatestIndexes()
//    {
//        //ComboBox sets(hDlg, IDC_COMBO1);
//        //ComboBox msnlist(hDlg, IDC_COMBO2);

//        //HKEY hkey;
//        //DWORD index1 = sets.getSellectionIndex(), index2 = msnlist.getSellectionIndex(), dwDisposition;
//        //RegCreateKeyEx(HKEY_CURRENT_USER, "Software\\MADia\\MenuWin", 0, "", 0, KEY_READ | KEY_WRITE, NULL, &hkey, &dwDisposition);
//        //RegSetValueEx(hkey, "Index1", 0, REG_DWORD, (LPBYTE) & index1, sizeof(index1));
//        //RegSetValueEx(hkey, "Index2", 0, REG_DWORD, (LPBYTE) & index2, sizeof(index2));
//        //RegCloseKey(hkey);
//    }


//    bool setGameData()
//    {
//        ComboBox crafts = new ComboBox(MenuWin.IDC_CRAFT);
//        //for (OBJECT_DATA d = OBJECT_DATA.GetFirstItem(); d!=null; d = d.Next())
//        foreach (OBJECT_DATA d in OBJECT_DATA.Datas)
//            if (d.GetClass() == OBJECT_DATA.OC_CRAFT && d.Side == 0)
//                crafts.AddString<OBJECT_DATA>(d.FullName, d);

//        if (crafts.GetCount()!=0)
//        {
//            crafts.SetCurSelection("Human_BF4");
//            initWeapons(crafts.GetSelection<CRAFT_DATA>());
//            // EnableWindow(crafts.m_hWnd, false);
//            return true;
//        }
//        else
//        {
//            errorBox("No one numan side craft found");
//            return false;
//        }
//    }

//    private void errorBox(string v)
//    {
//        throw new System.NotImplementedException();
//    }

//    void initWeapons(CRAFT_DATA cd)
//    {

//        myResult.CratftName = cd.Name;
//        ComboBox[] wp = new ComboBox[] { new ComboBox(MenuWin.IDC_WP1), new ComboBox(MenuWin.IDC_WP2),new  ComboBox(MenuWin.IDC_WP3) };
//        string[] defWpnNames = new string[] { "PPC", "AC", "GM" };

//        for (int i = 0; i < 3; ++i)
//        {
//            wp[i].Reset();
//            //wp[i].AddString<crc32>(">> None <<", Storm.CRC32.CRC_NULL);
//            wp[i].AddStringLP(">> None <<", Storm.CRC32.CRC_NULL);
//            // EnableWindow(wp[i].m_hWnd, false);

//            LayoutEnumerator.enumWeapons<WPN_DATA>(cd, SUBOBJ_DATA.Datas, i, new CbWeaponEnumer(wp[i]));

//            wp[i].SetCurSelection(defWpnNames[i]);
//            readWpnComboResult(wp[i].m_hWnd, i);
//            //EnableWindow(wp[i].m_hWnd, wp[i].GetCount() > 1);
//            // EnableWindow(wp[i].m_hWnd, false);
//        }
//    }

//    // called before dialog window destroyed
//    public virtual void OnDestroy() { }

//    void UpdateCampain(bool start = false)
//    {
//        ComboBox msnlist = new ComboBox(MenuWin.IDC_COMBO2);
//        msnlist.Reset();


//        iUnifiedVariableDB db = MenuWin.openSelectedUdb(new ComboBox(MenuWin.IDC_COMBO1), myPath);
//        if (db!=null)
//        {
//            iUnifiedVariableContainer root = db.GetRootTpl<iUnifiedVariableContainer>();
//            if (root!=null)
//            {
//                iUnifiedVariableContainer missions = root.GetVariableTpl<iUnifiedVariableContainer>("Events");
//                if (missions!=null)
//                {
//                    for (DWORD h = 0; (h = missions.GetNextHandle(h))!=0;)
//                    {
//                        iUnifiedVariableContainer msn =
//                            missions.GetVariableTpl<iUnifiedVariableContainer>(h);

//                        if (msn!=null)
//                        {
//                            iUnifiedVariableInt type =
//                                msn.GetVariableTpl<iUnifiedVariableInt>("Type");

//                            if (type!=null)
//                            {
//                                //char name[128];
//                                string name="";
//                                msnlist.AddStringLP(missions.GetNameByHandle(ref name, h));
//                            }
//                        }
//                    }
//                }
//            }
//        }

//        if (msnlist.GetCount()!=0)
//        {
//            msnlist.SetCurSelection(0);
//            if (start)
//            {
//                DWORD idx;
//                setLatestIndexes("Index2", out idx);
//                if (idx < msnlist.GetCount())
//                    msnlist.SetCurSelection(idx);
//            }
//        }

//        EnableWindow(GetDlgItem(hDlg, IDOK), msnlist.GetCount());
//    }

//    void OnOk()
//    {
//        ComboBox sets=new ComboBox(MenuWin.IDC_COMBO1);
//        ComboBox msnlist = new ComboBox(MenuWin.IDC_COMBO2);

//        bool ok = sets.GetSelString(ref myResult.DbName) &&
//            msnlist.GetSelString(ref myResult.MissionName);

//        //if (ok) *__strrchr(myResult.DbName, '.') = 0;

//        storeLatestIndexes();

//        EndDialog(ok);
//    }

//    private void EndDialog(bool ok)
//    {
//        throw new System.NotImplementedException();
//    }

//    void readWpnComboResult(int hCtrl, int wpIndex)
//    {
//        myResult.Weapon[wpIndex] = new ComboBox(hCtrl).GetSelection<crc32>();
//    }

//    // dialog controls events handler
//    virtual bool OnCommand(HWND hDlg, int Code, int idCtrl, HWND hCtrl)
//    {
//        switch (idCtrl)
//        {
//            case IDOK:
//                OnOk(hDlg);
//                break;

//            case IDCANCEL:
//                EndDialog(hDlg, false); break;

//            case IDC_COMBO1:
//                if (Code == CBN_SELCHANGE)
//                    UpdateCampain(hDlg);
//                break;

//            case IDC_CRAFT:
//                if (Code == CBN_SELCHANGE)
//                    initWeapons(hDlg, ComboBox(hCtrl).GetSelection<CRAFT_DATA*>());
//                break;

//            case IDC_WP1:
//            case IDC_WP2:
//            case IDC_WP3:
//                Assert((IDC_WP1 + 1) == IDC_WP2 && (IDC_WP2 + 1) == IDC_WP3);
//                if (Code == CBN_SELCHANGE)
//                    readWpnComboResult(hCtrl, idCtrl - IDC_WP1);
//                break;
//        }

//        return false;
//    }

//    public virtual void AddRef() { }
//    public virtual int Release() { return 1; }

//    internal bool display()
//    {
//        throw new System.NotImplementedException();
//    }
//}

public class UnityMenuDevButton
{
    private Button myButton;
    private IDialog myDialogClass;
    private string name;

    public UnityMenuDevButton(IDialog DialogClass, UIDocument myDialog, string name)
    {
        myDialogClass = DialogClass;
        myButton = myDialog.rootVisualElement.Q<VisualElement>("MenuWinDialog").Q<VisualElement>("Dialog").Q<Button>(name);
        Debug.Log(string.Format("Me new button {0}, found self? {1}", name, (myButton != null).ToString()));
        this.name = name;
        //myButton.RegisterValueChangedCallback(onValueChanged);
        myButton.clicked += onClick;
    }

    private void onClick()
    {
        Debug.Log("Click on name! " + name);
        myDialogClass.OnCommand(0, 0, name);
    }
}
public class UnityMenuDevCombobox
{
    const int CB_ERR = -1;
    public const int CBN_SELCHANGE = 0;
    public const int badIndex = CB_ERR;
    public string name;
    //private GameObject myGOBJ;
    //private List<Dropdown.OptionData> myDropDownOptions;
    private DropdownField myDropdownField;
    private IDialog myDialogClass;
    private List<object> storage = new List<object>();

    public UnityMenuDevCombobox(IDialog DialogClass,UIDocument myDialog, string name)
    {
        myDialogClass = DialogClass;
        myDropdownField = myDialog.rootVisualElement.Q<VisualElement>("MenuWinDialog").Q<VisualElement>("Dialog").Q<DropdownField>(name);
        Debug.Log(string.Format("Me new ComboBox {0}, found self? {1}", name, (myDropdownField != null).ToString()));
        this.name = name;
        myDropdownField.RegisterValueChangedCallback(onValueChanged);
    }

    private void onValueChanged(ChangeEvent<string> evt)
    {
        //Debug.Log($"Toggle changed. Old value: {evt.previousValue}, new value: {evt.newValue}");
        myDialogClass.OnCommand(CBN_SELCHANGE, 0, name);
    }

    public int AddStringLP(string str, object data = null, bool setcursel = false)
    {
        //var myDropDownChoices = myDropdownField.choices;
        //myDropDownChoices.Add(str);
        myDropdownField.choices.Add(str);
        storage.Add(data);
        //myDropdownField.choices= myDropDownChoices;
        return myDropdownField.choices.Count - 1;
    }

    public int findString(string str)
    {
        var myDropDownOptions = myDropdownField.choices;
        for (int i = 0; i < myDropDownOptions.Count; i++)
        {
            if (myDropDownOptions[i] == str) return i;
        }
        return -1;
    }

    public bool GetSelString(ref string buffer)
    {
        buffer = myDropdownField.value;
        if (buffer == null) return false;
        return true;
    }

    public object getSelectionData()
    {
        var index = getSellectionIndex();
        return storage[index];
    }

    public int getSellectionIndex()
    {
        return myDropdownField.index;
    }

    public int GetCount()
    {
        return myDropdownField.choices.Count;
    }

    internal int SetCurSelection(uint idx)
    {
        myDropdownField.index = (int)idx;
        return (int)idx;
    }

    internal int SetCurSelection(string str)
    {
        int index = findString(str);
        return index != badIndex ? SetCurSelection((uint)index) : CB_ERR;
    }

    internal T GetSelection<T>()
    {
        return (T)getSelectionData();
    }

    public void Reset()
    {
        myDropdownField.index = 0;
        myDropdownField.choices.Clear();
    }

    internal void AddString<T>(string fullName, T d, bool setcursel = false) where T : class
    {
        AddStringLP(fullName, d, setcursel);
    }


}

/// <summary>
/// interface IDialog
/// </summary>
public interface IDialog : IObject
{
    //virtual HWND create(HWND hParent = HWND_DESKTOP, HINSTANCE hModule = 0);
    //virtual int display(HWND hParent = HWND_DESKTOP, HINSTANCE hModule = 0);

    //// dialog template resource identifier
    //virtual int GetResourceID()=0;

    //// called before dialog begins to display
    //virtual bool OnInit(HWND hDlg) { return false; }

    //// called before dialog window destroyed
    //virtual void OnDestroy(HWND hDlg) { }

    //// dialog controls events handler
    virtual bool OnCommand(int Code, int idCtrl, string hCtrl) { return false; }

    //// dialog controls notifications handler
    //virtual bool OnNotify(HWND hDlg, int idCtrl, NMHDR* msgHdr) { return false; }

};