using UnityEngine;
using crc32 = System.UInt32;

public class CmdSendPlayerMessage : CmdSendMessage, IMenuHandler
{
     
    ~CmdSendPlayerMessage()
    {
        if (myMenu!=null) myMenu.unregisterHandler(this);
    }

    public override string getName()
    {
        return "SendMessage";
    }


    public override  bool isParsingCorrect()
    {
        myHashCode = Hasher.HshString(myCode);
        return base.isParsingCorrect();
    }

    public override bool restart()
    {
        myFirstTime = true;
        myMenu.registerHandler(this);
        return base.restart();
    }

    public bool notifySelect(crc32 code)
    {
        return mySuccess = (myHashCode == code);
    }

    public override bool exec()
    {
        //TODO удалить для корректного добавления сообщения в меню, добавить для отправки сообщения "сразу", без ожидания реакции игрока. Убрать после реализации радиокоманд игрока
        return base.exec(); 
        if (mySender.isPlayable() == false) return base.exec();

        if (myFirstTime)
        {
            myFirstTime = false;
            myMenu.addMenuItem(myCode, null, null);
        }
        else if (mySuccess)
        {
            myMenu.deleteMenuItem(myHashCode);
            send();
            return true;
        }

        float scale = myTimer.getDelta();
        switch (myInfo.update(scale, false))
        {
            case MessageProcessResult.rtSendAndExit:
                myMenu.deleteMenuItem(myHashCode);
                myMenu.addMenuItem(myCode, null, null);
                resetTimers();
                break;
            case MessageProcessResult.rtExit:
                myMenu.deleteMenuItem(myHashCode);
                return true;
        }
        return false;
    }

    public override bool setDefaults(IQuery tm)
    {
        if ((myMenu = (IMenuService) tm.Query(IMenuService.ID))!=null)
            myMenu.registerHandler(this);
        return myMenu!=null && base.setDefaults(tm);
    }

    bool mySuccess = false ;
    bool myFirstTime = true;
    IMenuService myMenu;
    crc32 myHashCode;

};
