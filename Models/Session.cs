namespace InfoSystem
{
    /// <summary>
    /// Holds the currently logged-in user's session data.
    /// </summary>
    public static class Session
    {
        public static int    UserId   { get; set; }
        public static string Username { get; set; }
        public static string FullName { get; set; }
        public static string Role     { get; set; }

        public static void Clear()
        {
            UserId   = 0;
            Username = "";
            FullName = "";
            Role     = "";
        }
    }
}
