using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.ServiceBase;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        /// <summary>
        ///     编辑指定据点
        /// </summary>
        /// <param name="id">据点 ID</param>
        /// <param name="requestDto">据点相关属性</param>
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> UpdateOneById(string id,
            [NotNull] NormalPointCreateOrUpdateOneRequestDto requestDto)
        {
            var normalPoint = await _dbContext.NormalPoints
                .Include(p => p.DeveloperPoints)
                .Include(p => p.PublisherPoints)
                .Include(p => p.SeriesPoints)
                .Include(p => p.GenrePoints)
                .Include(p => p.TagPoints)
                .Include(p => p.MajorPlatformPoints)
                .Include(p => p.MinorPlatformPoints)
                .Include(p => p.SteamStoreNames)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (normalPoint == null)
                return NotFound();

            if (!Helpers.IsTrustedUrl(requestDto.BackgroundImage))
                return this.BadRequest(nameof(requestDto), nameof(requestDto.BackgroundImage), Errors.Invalid);

            if (!Helpers.IsTrustedUrl(requestDto.AvatarImage))
                return this.BadRequest(nameof(requestDto), nameof(requestDto.AvatarImage), Errors.Invalid);

            var editorStaffClaim = await _userManager.GetStaffClaimAsync(User.Identity.GetUserId());
            if (editorStaffClaim == StaffClaim.Operator)
            {
                if (string.IsNullOrWhiteSpace(requestDto.EnglishName))
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.EnglishName), Errors.Required);

                if (requestDto.PreferredName == null)
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.PreferredName), Errors.Required);

                if (string.IsNullOrWhiteSpace(requestDto.IdCode))
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.IdCode), Errors.Required);

                if (!Regex.IsMatch(requestDto.IdCode, @"^[A-Z0-9]{5}$"))
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.IdCode), Errors.Invalid);

                if (requestDto.IdCode != normalPoint.IdCode &&
                    await _dbContext.NormalPoints.AnyAsync(u => u.IdCode == requestDto.IdCode))
                    return this.BadRequest(nameof(requestDto), nameof(requestDto.IdCode), Errors.Duplicate);

                normalPoint.EnglishName = requestDto.EnglishName;
                normalPoint.IdCode = requestDto.IdCode;
                normalPoint.PreferredName = requestDto.PreferredName.Value;
                if (normalPoint.Type == NormalPointType.Genre || normalPoint.Type == NormalPointType.Manufacturer)
                {
                    if (requestDto.NameInSteamStore == null)
                        return this.BadRequest(nameof(requestDto), nameof(requestDto.NameInSteamStore), Errors.Required);

                    var nameStrings =
                        requestDto.NameInSteamStore.Split(';')
                            .Select(n => n.Trim())
                            .Where(n => !string.IsNullOrWhiteSpace(n));
                    var names = new List<SteamStoreName>();
                    foreach (var nameString in nameStrings)
                    {
                        var name =
                            await _dbContext.SteamStoreNames.Where(n => n.Name == nameString).SingleOrDefaultAsync() ??
                            _dbContext.SteamStoreNames.Create();
                        name.Name = nameString;
                        names.Add(name);
                    }
                    normalPoint.SteamStoreNames = names;
                }
            }

            normalPoint.BackgroundImage = requestDto.BackgroundImage;
            normalPoint.AvatarImage = requestDto.AvatarImage;
            normalPoint.ChineseName = requestDto.ChineseName;
            normalPoint.ChineseAliases = requestDto.ChineseAliases;
            normalPoint.EnglishAliases = requestDto.EnglishAliases;
            normalPoint.Description = requestDto.Description.Length > 256
                ? requestDto.Description.Substring(0, 256)
                : requestDto.Description;

            if (normalPoint.Type == NormalPointType.Game &&
                !await PopulateGamePointAttributes(normalPoint, requestDto, editorStaffClaim, true))
            {
                return BadRequest(ModelState);
            }
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}