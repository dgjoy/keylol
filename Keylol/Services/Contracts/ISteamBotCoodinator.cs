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

        [OperationContract]
        Task UpdateBots(IEnumerable<SteamBotVM> vms);

        [OperationContract]
        Task<UserDTO> GetUserBySteamId(long steamId);

        [OperationContract]
        Task<bool> BindSteamUserWithBindingToken(long userSteamId, string code, string botId);

        [OperationContract]
        Task<bool> BindSteamUserWithLoginToken(long userSteamId, string code);

        [OperationContract(IsOneWay = true)]
        Task BroadcastBotOnFriendAdded(string botId);
    }
    
    public interface ISteamBotCoodinatorCallback
    {
        [OperationContract(IsOneWay = true)]
        void DeleteSteamFriend(string botId, long steamId);
    }
}