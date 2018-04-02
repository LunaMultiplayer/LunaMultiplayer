namespace LunaClient.Localization.Structures
{
    public class AdminWindowText
    {
        public string Title { get; set; } = "Administration";
        public string Password { get; set; } = "Server admin password";

        public string BanConfirmDialogTitle { get; set; } = "Ban player";
        public string BanText { get; set; } = "Do you really want to ban this player?";

        public string KickConfirmDialogTitle { get; set; } = "Kick player";
        public string KickText { get; set; } = "Do you really want to kick this player?";
    }
}
