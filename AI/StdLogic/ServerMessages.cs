public sealed class ServerMessages
{

    // ---------------------------------------------------------------------------
    // common strings
    // ---------------------------------------------------------------------------

    public const string sLocal = "Local";
    public const string sLocals = "Locals";
    // ---------------------------------------------------------------------------
    // X-console messages
    // ---------------------------------------------------------------------------

    // =========== standart ============

    // client (admin) console messages
    public const string sAdminUnableKick = "server: unable to kick %s";
    public const string sAdminUnableFind = "server: unable to find client with ID %d";
    public const string sAdminLogged = "server: client %d is now logged";
    public const string sAdminBadIP = "server: unable to resolve IP address %s";
    public const string sSrvSeparator = "-------------------------";
    public const string sSrvSeparatorTotal = "----- Total : %3d ----";
    public const string sUnknownParameter = "server: unknown parameter \"%s\"";
    public const string sSrvBadPassword = "server: incorrect password \"%s\"";
    public const string sSrvInfoUptime = "server: uptime %2d:%2d:%2d";
    public const string sSrvInfoContacts = "server: groups=%d contacts=%d clients=%d";
    public const string sSrvClientInfo = "%d %i.%i.%i.%i %s %2d:%2d";
    public const string sSrvBanInfo = "%d %d.%d.%d.%d %s.%s.%s.%s";
    public const string sAdminBanListSaved = "server: ban list saved, %d addresses";
    public const string sAdminBanListNSaved = "server: ban list save failed";
    public const string sAdminPlayerMuted = "server: client %d muted";
    public const string sAdminPlayerUnMuted = "server: client %d unmuted";
    public const string sSrvLogSizeSet = "server: max log size set to %d bytes";
    public const string sSrvLogTimeSet = "server: max log time set to %d seconds";
    public const string sServerLogFileReopened = "server: log file reopened";
    public const string sSrvConfigSet = "server: config set to \"%s\"";

    // host console message
    public const string sConnectionWasBanned = "server: connection from %d.%d.%d.%d was banned";

    public const string sServerPause = "srv_pause";
    public const string sServerStop = "srv_stop";

    // ---------------------------------------------------------------------------
    // ailog messages

    // =========== standart =============
    public const string sStartDelimiter = "------------------------------------------------------------";
    public const string sAiConnected = "ClientConnect";
    public const string sAiConnectFail = "ClientConnectFail";
    public const string sAiDisconnected = "ClientDisconnect";
    public const string sInitGame = "InitGame";
    public const string sClientScores = "show_scores";
    public const string sTeamSay = "cl_tsay";


    // =========== teamplay =============
    public const string sAssistInfo = "Assist";
    public const string sTAssistInfo = "TAssist";
    public const string sKillInfo = "Kill";
    public const string sTKillInfo = "TKill";
    public const string sLostInfo = "Lost";
    public const string sRewardInfo = "Reward";
    public const string sConfirmInfo = "Confirm";
    public const string sCaptureInfo = "Capture";
    public const string sTeamInfo = "TeamInfo";
    public const string sGameInfo = "GameInfo";
    public const string sClientInfo = "ClientInfo";
    public const string sRowInfo = "Row";

    //StdStandartAi:
    public const string sServerAccel = "srv_accel";
}