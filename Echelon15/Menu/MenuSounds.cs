using System.Collections.Generic;
using UnityEngine;

public class MenuSounds
{
    public MenuSounds(ISound snd)
    {
        myVoice = snd.CreateVoice();
        myDb = myVoice.OpenVoiceDB("menu.sfx");
    }
    public void playSound(string name)
    {
        IWave wave = myVoice.CreateWave(myDb, name);
        if (wave!=null)
        {
            myWaves.Add(wave);
            wave.Play(false, myVolume);
        }
    }

    public void update()
    {
        for (int i = 0; i < myWaves.Count;)
            if (myWaves[i].IsPlaying())
                ++i;
            else
                myWaves.RemoveAt(i);
    }

    public void setVolume(int vol) { myVolume = vol; }

    IVoice myVoice;
    IObject myDb;

    List<IWave> myWaves;
    int myVolume = 5;
};
