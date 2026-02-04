using System;

public class DummySound: ISound
{
    public static ISound  CreateDummySound()
    {
        return new DummySound();
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public float convertVolume(int vol)
    {
        throw new System.NotImplementedException();
    }

    public I3DSound Create3D(int volume = 9, bool can_edit = true)
    {
        throw new System.NotImplementedException();
    }

    public IStreamAudio CreateStreamAudio(IStreamSoundData ssd, int nSeconds)
    {
        throw new System.NotImplementedException();
    }

    public IVoice CreateVoice(bool fake = false)
    {
        throw new System.NotImplementedException();
    }

    public int GetMasterVolume()
    {
        throw new System.NotImplementedException();
    }

    public IStreamSoundData OpenWaveFile(string name, bool loop)
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    public void SetMasterVolume(int i)
    {
        throw new System.NotImplementedException();
    }

    public void Update(float scale)
    {
        throw new System.NotImplementedException();
    }

    internal static I3DSound CreateDummy3DSound()
    {
        return (I3DSound) new Dummy3DSound();
    }
}

public class Dummy3DSound
{
    //STUB 
}