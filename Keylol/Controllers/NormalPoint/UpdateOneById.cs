using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
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
        public async Task<IHttpActionResult> UpdateOneById(string id, NormalPointCreateOrUpdateOneRequestDto requestDto)
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

            if (requestDto == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!Helpers.IsTrustedUrl(requestDto.BackgroundImage))
            {
                ModelState.AddModelError("vm.BackgroundImage", "不允许使用可不信图片来源");
                return BadRequest(ModelState);
            }

            if (!Helpers.IsTrustedUrl(requestDto.AvatarImage))
            {
                ModelState.AddModelError("vm.AvatarImage", "不允许使用可不信图片来源");
                return BadRequest(ModelState);
            }

            var editorStaffClaim = await _userManager.GetStaffClaimAsync(User.Identity.GetUserId());
            if (editorStaffClaim == StaffClaim.Operator)
            {
                if (string.IsNullOrEmpty(requestDto.EnglishName))
                {
                    ModelState.AddModelError("vm.EnglishName", "据点英文名称必填");
                    return BadRequest(ModelState);
                }
                if (requestDto.PreferredName == null)
                {
                    ModelState.AddModelError("vm.PreferredName", "名称语言偏好必填");
                    return BadRequest(ModelState);
                }
                if (string.IsNullOrEmpty(requestDto.IdCode))
                {
                    ModelState.AddModelError("vm.IdCode", "据点识别码必填");
                    return BadRequest(ModelState);
                }
                if (!Regex.IsMatch(requestDto.IdCode, @"^[A-Z0-9]{5}$"))
                {
                    ModelState.AddModelError("vm.IdCode", "识别码只允许使用 5 位数字或大写字母");
                    return BadRequest(ModelState);
                }
                if (requestDto.IdCode != normalPoint.IdCode &&
                    await _dbContext.NormalPoints.AnyAsync(u => u.IdCode == requestDto.IdCode))
                {
                    ModelState.AddModelError("vm.IdCode", "识别码已经被其他据点使用");
                    return BadRequest(ModelState);
                }
                normalPoint.EnglishName = requestDto.EnglishName;
                normalPoint.IdCode = requestDto.IdCode;
                normalPoint.PreferredName = requestDto.PreferredName.Value;
                if (normalPoint.Type == NormalPointType.Genre || normalPoint.Type == NormalPointType.Manufacturer)
                {
                    if (requestDto.NameInSteamStore == null)
                    {
                        ModelState.AddModelError("vm.NameInSteamStore", "商店匹配名必填");
                        return BadRequest(ModelState);
                    }
                    var nameStrings =
                        requestDto.NameInSteamStore.Split(';')
                            .Select(n => n.Trim())
                            .Where(n => !string.IsNullOrEmpty(n));
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