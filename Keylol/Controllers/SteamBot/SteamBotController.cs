﻿using System.Web.Http;
using Keylol.Filters;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Utilities;

namespace Keylol.Controllers.SteamBot
{
    /// <summary>
    ///     Steam 机器人 Controller
    /// </summary>
    [Authorize]
    [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
    [RoutePrefix("steam-bot")]
    public partial class SteamBotController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        ///     创建 <see cref="SteamBotController" />
        /// </summary>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        public SteamBotController(KeylolDbContext dbContext, KeylolUserManager userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
    }
}