using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;

namespace Keylol.Services.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof (ISteamBotCoodinatorCallback))]
    public interface ISteamBotCoodinator
    {
        [OperationContract]
        Task<IEnumerable<SteamBotDTO>> AllocateBots();

        [OperationContract(IsOneWay = true)]
        Task UpdateBots(IEnumerable<SteamBotVM> vms);

        [OperationContract]
        Task<UserDTO> GetUserBySteamId(string steamId);

        [OperationContract]
        Task<bool> BindSteamUserWithBindingToken(string userSteamId, string code, string botId);

        [OperationContract]
        Task<bool> BindSteamUserWithLoginToken(string userSteamId, string code);

        [OperationContract(IsOneWay = true)]
        Task BroadcastBotOnFriendAdded(string botId);
    }
    
    public interface ISteamBotCoodinatorCallback
    {
        [OperationContract(IsOneWay = true)]
        void RemoveSteamFriend(string botId, string steamId);
    }
}