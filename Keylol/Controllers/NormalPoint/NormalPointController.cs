using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Utilities;

namespace Keylol.Controllers.NormalPoint
{
    /// <summary>
    ///     普通据点 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("normal-point")]
    public partial class NormalPointController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        ///     创建 <see cref="NormalPointController" />
        /// </summary>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        public NormalPointController(KeylolDbContext dbContext, KeylolUserManager userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        private static string GetPreferredName(Models.NormalPoint point)
        {
            switch (point.PreferredName)
            {
                case PreferredNameType.Chinese:
                    return point.ChineseName;

                case PreferredNameType.English:
                    return point.EnglishName;

                default:
                    throw new ArgumentOutOfRangeException(nameof(point));
            }
        }

        private async Task<bool> PopulateGamePointAttributes(Models.NormalPoint normalPoint,
            NormalPointCreateOrUpdateOneRequestDto requestDto, string editorStaffClaim, bool keepSteamAppId = false)
        {
            if (requestDto.DisplayAliases == null)
            {
                ModelState.AddModelError("vm.DisplayAliases", "游戏据点必须填写别名");
                return false;
            }
            if (requestDto.CoverImage == null)
            {
                ModelState.AddModelError("vm.CoverImage", "游戏据点必须填写封面图片");
                return false;
            }
            if (!requestDto.CoverImage.IsTrustedUrl())
            {
                ModelState.AddModelError("vm.CoverImage", "不允许使用可不信图片来源");
                return false;
            }
            if (requestDto.DeveloperPointsId == null)
            {
                ModelState.AddModelError("vm.DeveloperPointsId", "游戏据点必须填写开发商据点");
                return false;
            }
            if (requestDto.PublisherPointsId == null)
            {
                ModelState.AddModelError("vm.PublisherPointsId", "游戏据点必须填写发行商据点");
                return false;
            }
            if (requestDto.GenrePointsId == null)
            {
                ModelState.AddModelError("vm.GenrePointsId", "游戏据点必须填写类型据点");
                return false;
            }
            if (requestDto.TagPointsId == null)
            {
                ModelState.AddModelError("vm.TagPointsId", "游戏据点必须填写特性据点");
                return false;
            }
            if (requestDto.MajorPlatformPointsId == null)
            {
                ModelState.AddModelError("vm.MajorPlatformPointsId", "游戏据点必须填写主要平台据点");
                return false;
            }
            if (requestDto.MinorPlatformPointsId == null)
            {
                ModelState.AddModelError("vm.MinorPlatformPointsId", "游戏据点必须填写次要平台据点");
                return false;
            }
            if (requestDto.SeriesPointsId == null)
            {
                ModelState.AddModelError("vm.SeriesPointsId", "游戏据点必须填写系列据点");
                return false;
            }
            if (editorStaffClaim == StaffClaim.Operator)
            {
                if (!keepSteamAppId)
                {
                    if (requestDto.SteamAppId == null)
                    {
                        ModelState.AddModelError("vm.SteamAppId", "游戏据点的 App ID 不能为空");
                        return false;
                    }
                    normalPoint.SteamAppId = requestDto.SteamAppId.Value;
                }
                if (requestDto.ReleaseDate == null)
                {
                    ModelState.AddModelError("vm.ReleaseDate", "游戏据点的面世日期不能为空");
                    return false;
                }
                normalPoint.ReleaseDate = requestDto.ReleaseDate.Value;
            }
            normalPoint.DisplayAliases = requestDto.DisplayAliases;
            normalPoint.CoverImage = requestDto.CoverImage;

            normalPoint.DeveloperPoints = await _dbContext.NormalPoints
                .Where(
                    p =>
                        p.Type == NormalPointType.Manufacturer &&
                        requestDto.DeveloperPointsId.Contains(p.Id))
                .ToListAsync();
            normalPoint.PublisherPoints = await _dbContext.NormalPoints
                .Where(
                    p =>
                        p.Type == NormalPointType.Manufacturer &&
                        requestDto.PublisherPointsId.Contains(p.Id))
                .ToListAsync();
            normalPoint.GenrePoints = await _dbContext.NormalPoints
                .Where(p => p.Type == NormalPointType.Genre && requestDto.GenrePointsId.Contains(p.Id))
                .ToListAsync();
            normalPoint.TagPoints = await _dbContext.NormalPoints
                .Where(p => p.Type == NormalPointType.Genre && requestDto.TagPointsId.Contains(p.Id))
                .ToListAsync();
            normalPoint.MajorPlatformPoints = await _dbContext.NormalPoints
                .Where(
                    p =>
                        p.Type == NormalPointType.Platform &&
                        requestDto.MajorPlatformPointsId.Contains(p.Id))
                .ToListAsync();
            normalPoint.MinorPlatformForPoints = await _dbContext.NormalPoints
                .Where(
                    p =>
                        p.Type == NormalPointType.Platform &&
                        requestDto.MinorPlatformPointsId.Contains(p.Id))
                .ToListAsync();
            normalPoint.SeriesPoints = await _dbContext.NormalPoints
                .Where(p => p.Type == NormalPointType.Genre && requestDto.SeriesPointsId.Contains(p.Id))
                .ToListAsync();

            return true;
        }

        private async Task<string> GenerateIdCode(string name)
        {
            var convertedName = string.Join("",
                name.ToUpper().Where(c => (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')));
            IEnumerable<string> possiblities;
            if (convertedName.Length < 5)
            {
                possiblities = Enumerable.Range(0, 20)
                    .Select(i =>
                        $"{convertedName}{Guid.NewGuid().ToString().Substring(0, 5 - convertedName.Length).ToUpper()}");
            }
            else
            {
                var combinations = convertedName.AllCombinations(5).Select(idCode => string.Join("", idCode));
                var randomList = Enumerable.Range(0, 20)
                    .Select(i => Guid.NewGuid().ToString().Substring(0, 5).ToUpper());
                possiblities = combinations.Concat(randomList);
            }
            foreach (var idCode in possiblities)
            {
                if (_dbContext.NormalPoints.Local.All(p => p.IdCode != idCode) &&
                    await _dbContext.NormalPoints.AllAsync(p => p.IdCode != idCode))
                    return idCode;
            }
            throw new Exception("无法找到可用的 IdCode");
        }
    }
}