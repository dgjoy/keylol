using System;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.DTO
{
    public class LoginLogDTO
    {
        public LoginLogDTO(LoginLog log)
        {
            Id = log.Id;
            Time = log.Time;
            Ip = log.Ip;
            UserId = log.User.Id;
        }

        public string Id { get; set; }
        public DateTime Time { get; set; }
        public string Ip { get; set; }
        public string UserId { get; set; }
    }
}