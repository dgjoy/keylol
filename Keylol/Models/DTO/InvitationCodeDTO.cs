using System;

namespace Keylol.Models.DTO
{
    public class InvitationCodeDTO
    {
        public InvitationCodeDTO(InvitationCode code, bool includeAll = false)
        {
            Id = code.Id;

            if (includeAll)
            {
                GenerateTime = code.GenerateTime;
                Source = code.Source;
            }
        }

        public string Id { get; set; }

        public DateTime? GenerateTime { get; set; }

        public string Source { get; set; }
    }
}