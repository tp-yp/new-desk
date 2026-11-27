namespace NewDesk.Shell;

/// <summary>User-facing text (Hebrew). Kept in one place for later localization.</summary>
internal static class Strings
{
    public const string AppName = "NewDesk";
    public const string Running = "NewDesk פועל";
    public const string TaskbarHidden = "שורת המשימות של Windows מוסתרת כרגע. בשלב הבא תבוא כאן שורת המשימות החדשה.";
    public const string HotkeyHint = "חירום: Ctrl+Alt+Shift+F12 מחזיר את Windows וסוגר.";
    public const string HotkeyMissing = "מקש החירום לא נרשם (תוכנה אחרת תופסת אותו). הכפתור למטה עדיין עובד.";
    public const string RestoreButton = "החזר את Windows וצא";
    public const string CrashButton = "בדיקה: קריסה מדומה";
    public const string CrashConfirm = "התוכנה תיסגר בכוח, כדי לבדוק שהשומר מחזיר את שורת המשימות ומפעיל אותה מחדש. להמשיך?";
    public const string SafeModeMessage = "מצב בטוח: Shift היה לחוץ בהפעלה, ולכן NewDesk לא שינה כלום ונסגר.";
    public const string NoWatchdogMessage = "לא נמצא תהליך השומר (NewDesk.Watchdog.exe) ליד התוכנה, ולכן NewDesk לא מסתיר את שורת המשימות ונסגר.";
}
