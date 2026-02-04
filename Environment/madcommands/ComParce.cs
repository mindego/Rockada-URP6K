using DWORD = System.UInt32;
using crc32 = System.UInt32;
///////////////////////////////////////////////////////////////////////////
//    storm commands language format specifcation
//     command [parameters]
//     +/- trigger
//     variable [value]
//     action action_name { }
//     action_name
///////////////////////////////////////////////////////////////////////////

/// <summary>
/// Абстрактный базовый класс для всех команд командного языка.
/// Содержит общие поля и методы для идентификации, сопоставления и управления командами.
/// </summary>
public abstract class ComBase
{
    /// <summary>
    /// Тип команды (например, команда, триггер, переменная и т.д.).
    /// </summary>
    public ComType ctype;

    //ComBase[] stack;
    /// <summary>
    /// Ссылка на связанную команду (используется для организации стека команд).
    /// </summary>
    ComBase stack;

    /// <summary>
    /// Имя команды.
    /// </summary>
    public string name;

    /// <summary>
    /// Хеш реального имени команды.
    /// </summary>
    public crc32 cname;   // my real name

    /// <summary>
    /// Хеш имени для парсинга (может отличаться от cname, например, для триггеров).
    /// </summary>
    public crc32 pname;   // my parcing name   cname!=pname when trigger

    /// <summary>
    /// Строка с подсказкой/описанием команды.
    /// </summary>
    public string help;    // constant help string

    /// <summary>
    /// Интерфейс обратной связи для переменных/команд.
    /// </summary>
    public CommLink link;

    /// <summary>
    /// Конструктор базового класса команды.
    /// </summary>
    /// <param name="nm">Имя команды.</param>
    /// <param name="cn">Хеш реального имени команды.</param>
    /// <param name="pn">Хеш имени для парсинга.</param>
    /// <param name="h">Строка с подсказкой/описанием команды.</param>
    /// <param name="l">Интерфейс обратной связи.</param>
    public ComBase(string nm, crc32 cn, crc32 pn, string h = null, CommLink l = null)
    {
        stack = null;
        help = h;
        link = l;
        ctype = ComType.CM_UNDEFINED;
        cname = cn;
        pname = pn;
        name = nm;
    }

    /// <summary>
    /// Устанавливает связанную команду (стек).
    /// </summary>
    /// <param name="st">Команда для установки в стек.</param>
    public void SetStacked(ComBase st) { stack = st; }

    /// <summary>
    /// Получает связанную команду (стек).
    /// </summary>
    /// <returns>Связанная команда.</returns>
    public ComBase GetStacked() { return stack; }

    /// <summary>
    /// Абстрактный метод выполнения команды.
    /// </summary>
    /// <param name="input">Входная строка для команды.</param>
    /// <param name="inv">Флаг инверсии (например, для триггеров).</param>
    /// <returns>Результат выполнения команды.</returns>
    public abstract string Exec(string input, bool inv);

    /// <summary>
    /// Получает имя команды (или его часть) в dest и длину возвращённой строки.
    /// </summary>
    /// <param name="dest">Строка, в которую будет записано имя.</param>
    /// <param name="size">Максимальная длина имени.</param>
    /// <returns>Длина возвращённой строки.</returns>
    public virtual int GetName(out string dest, int size)
    {
        dest = name.Substring(0, size);
        return dest.Length;
    }

    /// <summary>
    /// Проверяет, начинается ли строка s с имени команды.
    /// </summary>
    /// <param name="s">Строка для сопоставления.</param>
    /// <returns>Длина имени, если совпадает, иначе 0.</returns>
    public virtual int Match(string s)
    {
        //int pos = 0;
        //while (s[pos] && s[pos] == name[pos]) pos++;

        //return (name[pos] < s[pos]) ? 0 : (256 + s[pos] - name[pos] + pos * 256);
        return s.StartsWith(name) ? name.Length : 0; //TODO Что-то здесь не так. Надо проверить сравнивание триггеров
    }

    /// <summary>
    /// Заглушка для автодополнения команд.
    /// </summary>
    /// <param name="s">Введённая строка.</param>
    /// <param name="lim">Ограничение для подсказки.</param>
    public virtual void Suggest(string s, string lim)
    { // Реализовать подсказку по коммандам
    }

    /// <summary>
    /// Получает действие, связанное с командой (по умолчанию null).
    /// </summary>
    /// <returns>Объект действия или null.</returns>
    public virtual Action GetAction() { return null; }

    /// <summary>
    /// Деструктор. Освобождает ресурсы.
    /// </summary>
    ~ComBase()
    {
        Dispose();
    }

    private bool isDisposed = false;
    /// <summary>
    /// Освобождает ресурсы и проверяет, что стек пуст.
    /// </summary>
    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        if (stack != null) UnityEngine.Debug.Log("Disposing of " + name + " in stack: " + stack.name + " " + stack);
        Asserts.Assert(stack == null);
        name = null;
    }

    /// <summary>
    /// Неиспользуемый метод для установки имени команды.
    /// </summary>
    /// <param name="nm">Имя команды.</param>
    /// <param name="cn">Хеш реального имени.</param>
    /// <param name="pn">Хеш имени для парсинга.</param>
    void SetName(string nm, int cn, int pn) { }//Похоже, не используется
}
