using crc32 = System.UInt32;

public class MenuMusix
{
    public delegate IStreamSoundData DOpenMusic(string name, ISound snd);
    public MenuMusix()
    {
        myMusic = null;
        myName = CRC32.CRC_NULL;
    }

    public void play(string name, ISound snd, DOpenMusic openMusic)
    {
        crc32 crc = Hasher.HshString(name);
        if (myName != crc)
        {
            myName = crc;
            stop();

            if (myVolume != 0)
            {
                IStreamSoundData stream = openMusic(name, snd);
                myMusic = stream != null ? snd.CreateStreamAudio(stream, 6) : null;

                if (myMusic != null)
                {
                    myMusic.SetVolume(myVolume);
                    myMusic.Play();
                }
            }
        }
    }
    public void stop()
    {
        if (myMusic != null)
        {
            myMusic.Stop();
            myMusic.Release();
            myMusic = null;
            myName = CRC32.CRC_NULL;
        }
    }
    public void update()
    {
        if (myMusic != null)
            myMusic.Update();
    }
    public void setVolume(int vol) { myVolume = vol; if (myMusic != null) myMusic.SetVolume(vol); }

    private IStreamAudio myMusic;
    crc32 myName;
    int myVolume = 5;
};
