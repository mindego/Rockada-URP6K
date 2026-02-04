using UnityEngine;
using static pelems;

public static class putils
{
    /// <summary>
    /// Find closing bracket :<br></br>
    /// ex1: CloseBracket( "{ gsg }a", '{', '}')="}a";<br></br>
    /// ex2: CloseBracket( " gsg }a", '{', '}', true)="}a";<br></br>
    /// ex3: CloseBracket( "{ gsg {}a", '{', '}')=0;<br></br>
    /// </summary>
    /// <param name="s">String to examine</param>
    /// <param name="open">Opening Bracket</param>
    /// <param name="close">Closing Bracket</param>
    /// <param name="nOpened"></param>
    /// <param name="Comments"></param>
    /// <returns></returns>

    public static string CloseBracket(string s, char open, char close, int nOpened = 0, bool Comments = true)
    {
        if (Comments) s = next_char(s);
        //for (int x = *s; x; x = *s)
        //{
        //    nOpened += (x == open) ? 1 : (x == close) ? -1 : 0;
        //    if (nOpened==0) return s;
        //    s = Comments ? s + 1 : next_char(s + 1);
        //}
        int cnt = 0;
        for (; ; )
        {
            if (cnt++ >= 4069) { Debug.Log("too many chars processed!"); break; }
            char x = s[0];
            nOpened += (x == open) ? 1 : (x == close) ? -1 : 0;
            if (nOpened == 0) return s;
            //s = Comments ? s.Substring(1, s.Length - 1) : next_char(s.Substring(1, s.Length - 1));
            s = Comments ? s.Remove(0, 1) : next_char(s.Remove(0, 1));
            if (s.Length == 0) break;
        }
        return null;
    }

    /// <summary>
    /// Find closing bracket position :<br></br>
    /// ex1: CloseBracket( "{ gsg }a", '{', '}')=6;<br></br>
    /// ex2: CloseBracket( " gsg }a", '{', '}', true)=4;<br></br>
    /// ex3: CloseBracket( "{ gsg {}a", '{', '}')=-1;<br></br>
    /// </summary>
    /// <param name="s">String to examine</param>
    /// <param name="open">Opening Bracket</param>
    /// <param name="close">Closing Bracket</param>
    /// <param name="nOpened"></param>
    /// <param name="Comments"></param>
    /// <returns></returns>
    public static int CloseBracketPos(string s, char open, char close, int nOpened = 0, bool Comments = true)
    {
        int i = 0;
        if (Comments) i = next_char_pos(s);
        for (; i < s.Length; i++)
        {
            if (i >= 8000) { Debug.Log("too many chars processed!"); break; }
            char x = s[i];
            nOpened += (x == open) ? 1 : 0;
            nOpened -= (x == close) ? 1 : 0;
            if (nOpened == 0) return i;
        }
        return -1;
    }

    /// <summary>
    /// Parse line ( and copy to dest if (dest!=0)<br></br>
    /// Ex: while ( *parse ) { parse = GetLine( parse, line, 250); ProcessLine(line); }
    /// </summary>
    /// <param name="inText"></param>
    /// <param name="dest"></param>
    /// <param name="dest_size"></param>
    /// <param name="skip_empty"></param>
    /// <returns></returns>

    public static string GetLine(string inText, ref string dest, int dest_size = 0, bool skip_empty = true)
    {
        if (inText == null & inText == "") return "";

        if (skip_empty)
            inText = skipEols(inText);

        if (dest != null)
        {  // copy string to dest
            Debug.Log("GetLine: copy string to dest not implemented yet");
            //for (; --dest_size && !IsEol(*in); ++dest, ++in)
            //    *dest = *in;
            //*dest = 0;
            //TODO Реализовать копирование строки в dest 
        }
        else
        { // skip string 
            int index = 0;
            while (!IsEol(inText[index]))
            {
                index++;
                //inText = inText.Substring(++index, inText.Length - index);
                //inText = inText.Substring(1, inText.Length - 1);
            }
            if (index != 0) inText = inText.Substring(0, index + 1);
        }
        if (skip_empty)
            inText = skipEols(inText);
        else
            if (IsEolChar(inText[0]))
        {
            int index = (IsEolChar(inText[1]) && inText[1] != inText[0]) ? 2 : 1;
            inText = inText.Substring(index, inText.Length - index);
        }
        return inText;
    }


    public static int next_char_pos(string text, bool CppComments = true)
    {
        //Debug.Log("Trimming [" + text + "] comments" + CppComments);
        int i = 0;
        string otext = text;
        //for (i = 0; i < text.Length;)
        for (; ; )  //Этот цикл - для уборки множественных строк с комментариями
        {

            while (text.Length > 0 && is_space(text[0])) { text = text.Remove(0, 1); i++; }

            if (text.Length == 0)
            {
                Debug.LogError("text is empty");
                //throw new System.Exception("text is empty");
                return -1;
            }
            if (CppComments && text[0] == '/' && text[1] == '/')
            {
                string dest = null;
                //string comment = GetLine(text.Remove(i, 2), ref dest, 0, false);
                string comment = GetLine(text.Remove(0, 2), ref dest, 0, false);
                Debug.Log("Comment: [" + comment + "]");
                i += comment.Length + 2;
                text = text.Remove(0, comment.Length + 2);
            }
            else break;
        }
        //char[] c = otext.ToCharArray();
        //c[i] = '^';
        //Debug.Log("Trimming [" + new string(c) + "] @ " + i);
        return i;
    }


    public static string next_char(string text, bool CppComments = true)
    {
        int i = next_char_pos(text, CppComments);
        //return (i != -1) ? text.Remove(0, i) : text;
        return (i != -1) ? text.Remove(0, i) : null;
    }
    //public static string next_char(string text, bool CppComments = true)
    //{
    //    if (text.Length == 0) return null;
    //    int i;
    //    for (i = 0; i < text.Length; i++)
    //    {
    //        //Debug.Log(string.Format("{0} {1} {2} {3}", i, ((byte)text[i]),text.Length, text));
    //        while (is_space(text[i]))
    //        {
    //            //Debug.Log(string.Format("{0} of {1}",i++,text.Length));
    //            i++;
    //        }
    //        if (CppComments && text[i] == '/' && text[i + 1] == '/')
    //        {
    //            string dest = null;
    //            i += GetLine(text.Substring(i + 2, text.Length - (i + 2)), ref dest, 0, false).Length;
    //        }
    //        else break;
    //    }
    //    return text.Substring(i, text.Length - i);
    //    //return text.Remove(0, i+1);
    //}
}
