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
    [ServiceContract(Namespace = "http://xmlns.keylol.com/wcf/2015/09",
        CallbackContract = typeof (ISteamBotCoodinatorCallback))]
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
    }

    public interface ISteamBotCoodinatorCallback
    {
        void DeleteSteamFriend(string botId, long steamId);
    }
}