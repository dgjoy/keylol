using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    public class InvitationCodeDTO
    {
        public InvitationCodeDTO(InvitationCode code, bool includeGenerateTime = false)
        {
            Id = code.Id;

            if (includeGenerateTime)
                GenerateTime = code.GenerateTime;
        }

        public string Id { get; set; }

        public DateTime? GenerateTime { get; set; }
    }
}