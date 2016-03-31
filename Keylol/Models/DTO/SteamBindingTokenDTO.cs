namespace Keylol.Models.DTO
{
    /// <summary>
    ///     SteamBindingToken DTO
    /// </summary>
    public class SteamBindingTokenDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     绑定代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        ///     机器人 Steam ID 64
        /// </summary>
        public string BotSteamId64 { get; set; }
    }
}