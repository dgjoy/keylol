using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    [DataContract(Namespace = "http://xmlns.keylol.com/wcf/2015/09")]
    public class LoginLogDTO
    {
        public LoginLogDTO(LoginLog log)
        {
            Id = log.Id;
            Time = log.Time;
            Ip = log.Ip;
            UserId = log.User.Id;
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
        [DataMember]
        public string Ip { get; set; }
        [DataMember]
        public string UserId { get; set; }
    }
}