using UnityEngine;
using crc32 = System.UInt32;
public class MenuEnvironment : InputClient
{


    public ILog myLog;
    public CommandsApi myCommands;
    ITimer myTimer;
    public ITranslator2 myTranslator;

    //TPtr<ConsoleVisualizer> myConsole;
    //TPtr<ConsoleVisualizer> myChat;

    UnivarDB myGameDataDb = new UnivarDB();

    bool myMenuEnabled = false;
    bool myMenuEnabling = false;
    bool myInExitScreen = false;
    ScreenBackground myBG;

    //MenuWindow myWindow;

    public ISound mySound;
    public MenuMusix myMusic;
    public RendererApi myRenderer;
    IBill myBill;
    public EInput myInput;
    public ICtrlDialog myDialog;
    public MenuSounds mySounds;

    //MenuPalette myPalette;

    public IMenuVideo myVideo;

    //TRef<iMadSockets2Manager> mySockets;
    ITexturesDB myTex;

    public void playVideo(string name) { }
    void stopVideo() { }


    // Mad Input api 
    public virtual void processKeyPress(int _k, bool _c)
    {
        if (myTranslator == null) return;

        //TODO: Реализовать обработку нажатия клавиш в меню.
        //Debug.Log("processKeyPress for " + myTranslator);
        myTranslator.ProcessKeyPress(_k, _c?1:0);
    }

    bool openRenderer()
    {
        //char buffer[256];
        //wsprintf(buffer, "%s\\%s", GetPI()->RegKeyConfig(), "Video");
        //TRef<IRendererConfig> rdr = createObject<RendererConfigUV>();
        //char title[128];
        //wsprintf(title, "%s %s", GetPI()->Title(), GetPI()->StrVersion());

        //if (myWindow.create(HWND_DESKTOP, title, rdr->GetWidth(), rdr->GetHeight(), rdr->GetInWindow()))
        //{
        //    myRenderer = CreateRenderer(myWindow.getWindow(), rdr, myLog, myCommands, RENDERER_VERSION);
        //    if (myRenderer)
        //    {
        //        myWindow.setRenderer(myRenderer);
        //        myWindow.show(true);
        //        myBill = myRenderer->CreateBill();
        //    }
        //    else myLog->Message("ERROR: CreateRenderer failed.");
        //}
        //return myBill;
        string title = string.Format("{0} {1}", ProductDefs.GetPI().Title(), ProductDefs.GetPI().StrVersion());
        myRenderer = Renderer.CreateRenderer(null, (LOG)myLog, myCommands, RendererApi.RENDERER_VERSION);
        if (myRenderer != null)
        {
            //myBill = myRenderer.CreateBill();
            myBill = myRenderer.CreateBill();
        }
        else myLog.Message("ERROR: CreateRenderer failed.");
        return myBill != null;
    }
    void finalOpen() { }

    public MenuEnvironment()
    {
        myRenderer = null;
        myInput = null;
        myDialog = null;
        //myBill = null;
        mySound = null;
        myLog = LOG.createLOG("menu");
        myLog.OpenLogFile();
        myMusic = new MenuMusix();

        //myCommands = CreateCommands(myLog, ProductDefs.GetPI()->Fs(), "Profiles\\");
        myCommands = new Commands(myLog, ProductDefs.GetPI().Fs(), "Profiles\\");

        //myTimer = CreateTimer();
        myTranslator = Translator.CreateTranslator(myLog, myCommands);

        //myConsole = new ConsoleVisualizer(myTranslator, myCommands, createGameConsoleBehaviour);
        //myConsole->setLog(myLog);
        //myChat = new ConsoleVisualizer(myTranslator, myCommands, createChatConsoleBehaviour);

    }
    ~MenuEnvironment() { }

    public bool open(IJoys joys)
    {
        if (myGameDataDb.open(ProductDefs.GetPI().getHddFile("GameData.dat")))
        {
            //mySockets = CreateMadSockets2Manager(myLog, 20000);
            if (openRenderer())
            {

                myTex = myBill.CreateTexturesDB(Menu.TexturesPath);
                //myConsole->initGraphics(myBill, myTex);
                //myConsole->startEdit(">", 0);
                //myChat->initGraphics(myBill, myTex);
                //myBG.init(myBill, myTex);

                //mySound = ISoundApi.CreateSInstance(myWindow.getWindow(), myLog, myCommands);
                mySound = ISoundApi.CreateSInstance(null, myLog, myCommands);
                //myInput = EInput.createInput(myWindow.getWindow(), this, joys);
                myInput = EInput.createInput(null, this, joys);
                myInput.useJoysticks(0, 0);

                if (myBill != null && myInput != null && mySound != null)
                {
                    mySounds = new MenuSounds(mySound);
                    //myDialog = CreateMDialog(mctrls.MCTRLS_VERSION, myBill, myInput, "kursor", 0, myPalette);
                    myDialog = mctrls.CreateMDialog(mctrls.MCTRLS_VERSION, myBill, myInput, "kursor", 0, null);
                    if (myDialog != null)
                        finalOpen();
                    else
                        myLog.Message("ERROR: Dialog creation failed.");

                }
                else myLog.Message("ERROR: Sound or Input or Primitives failed to create.");
            }
        }
        else myLog.Message("ERROR: Can`t open config file");
        Debug.Log(DescribeVars());
        return myRenderer != null && myBill != null && mySound != null && myInput != null && myDialog != null;
    }

    private string DescribeVars()
    {
        return string.Format("myRenderer {0} myBill {1} mySound {2} myInput {3} myDialog {4}", myRenderer != null, myBill != null, mySound != null, myInput != null, myDialog != null);
    }
    public void startMusic() { myMusic.play("Menu.wav", mySound, openMusic); }
    public void stopMusic() { myMusic.stop(); }
    void openChat(string prompt, string param) { }
    public void enableMenu(bool on)
    {
        myDialog.EnableMouse(on);
        myMenuEnabled = on;
        myMenuEnabling = true;
    }
    public bool isMenuEnabled() { return !myMenuEnabling && myMenuEnabled; }
    public void enableExitScreen(bool on) { myInExitScreen = on; }
    bool isInExitScreen() { return myInExitScreen; }

    //public bool update(ref float time)
    //{
    //    //TODO Реализовать поведение меню.
    //    myMenuEnabling = false;
    //    //return !myWindow.isClosed();
    //    return true;
    //}

    public bool update(ref float time)
    {
        myMenuEnabling = false;
        //TODO Реализовать поведение меню.

        //bool isTyping = myConsole->isTyping() || myChat->isTyping();
        //int shiftState;

        //myTranslator->SetExec(!isTyping);
        //if (isTyping)
        //    shiftState = myTranslator->UseShift(true);

        //time = myTimer->Update();
        //if (mySockets) mySockets->Update();
        myInput.poll();
        mySound.Update(time);
        //myMusic.update();
        //mySounds.update();
        //myConsole->update(time);
        //myChat->update(time);

        //if (myVideo && !myVideo->update(time))
        //    stopVideo();

        //if (isTyping)
        //    myTranslator->UseShift(shiftState);

        //myMenuEnabling = false;
        //return !myWindow.isClosed();


        return true;
    }



    public bool beginDraw()
    {
        return myRenderer.StartFrame();
    }
    public void endDraw()
    {
        //if (isMenuEnabled())
        //{
        //    if (myInExitScreen)
        //        myBG.draw(myBill);
        //    if (myVideo==null)
        //    {
        //        myDialog.Draw();
        //        myBill.Begin();
        //        myBill.SetClipping(0);

        //        myBill.SetStyle(BillStyle(BLEND_ADD, true, true));
        //        myConsole->getFont()->Puts(SVec4(255, 128, 128, 128), FVec2(0.92, 0), GetPI()->StrVersion());
        //        myBill.End();
        //    }
        //}
        //else if (myVideo!=null)
        //    myVideo.draw();
        //myConsole.draw();
        //myChat.draw();
        //myRenderer.EndFrame();
        //}
    }

    static IStreamSoundData openMusic(string name, ISound s)
    {
        IStreamSoundData d = s.OpenWaveFile(ProductDefs.GetPI().getHddFile(name), true);
        //return d !=null? d : s.OpenWaveFile(getCdFile(name), true);
        return d;
    }
};

public interface InputClient
{
    public void processKeyPress(int key, bool state);
};

public interface IMenuVideo : IObject
{
    public bool update(float f);
    public void draw();
};

public interface ITexturesDB : IObject
{
    new public const uint ID = 0x7691A1E5;
    //public IObject CreateTexture(crc32 id);
    public Texture2D CreateTexture(crc32 id);
};
