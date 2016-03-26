namespace Keylol.Models.DTO
{
    public class SteamLoginTokenDTO
    {
        public SteamLoginTokenDTO(SteamLoginToken token)
        {
            Id = token.Id;
            Code = token.Code;
        }

        public string Id { get; set; }
        public string Code { get; set; }
    }
}