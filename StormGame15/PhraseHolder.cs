using System.Collections.Generic;
using UnityEngine;

public class PhraseHolder
{

    public PhraseHolder(string text, string name)
    {
        //text = pelems.skipEols(text);

        //string discard = null;
        //for (; text.Length>0;)
        //{
        //    string eol_ = putils.GetLine(text, ref discard, 0, true), eol = eol_;
        //    while (eol > text && IsEolChar(eol[-1])) --eol;
        //    if (eol > text)
        //    {
        //        cstr tab = __strchr(text, eol, '\t');
        //        tools::addString(myNames, text, tab - text);
        //        tools::addString(myWaves, tab + 1, eol - tab - 1);
        //        tools::addString(myRealNames, name, __strlen(name));
        //    }

        //    text = eol_;
        //}
        //TODO Реализовать хранилище фраз
        string[] lines = text.Split('\n');
        foreach (string line in lines)
        {
            string[] tokens = line.Split('\t');
            myNames.Add(tokens[0]);
            myWaves.Add(tokens.Length == 2 ? tokens[1]: null);
            myRealNames.Add(name);
        }
    }
    public string getText(int index)
    {
        return index < myNames.Count ? myNames[index] : null;
    }

    public string getName(int index)
    {
        return index < myRealNames.Count ? myRealNames[index] : null;
    }

    public string getWave(int index)
    {
        return index < myWaves.Count ? myWaves[index] : null;
    }

    public int getCount()
    {
        return myNames.Count;
    }

    List<string> myNames = new();
    List<string> myRealNames = new();
    List<string> myWaves = new();

};