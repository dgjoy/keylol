using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        /// <summary>
        ///     创建一个据点
        /// </summary>
        /// <param name="requestDto">据点相关属性</param>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (NormalPointDto))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> CreateOneManually(
            NormalPointCreateOrUpdateOneRequestDto requestDto)
        {
            if (requestDto == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (requestDto.IdCode == null ||
                !Regex.IsMatch(requestDto.IdCode, @"^[A-Z0-9]{5}$"))
            {
                ModelState.AddModelError("vm.IdCode", "识别码只允许使用 5 位数字或大写字母");
                return BadRequest(ModelState);
            }

            if (await DbContext.NormalPoints.AnyAsync(u => u.IdCode == requestDto.IdCode))
            {
                ModelState.AddModelError("vm.IdCode", "识别码已经被其他据点使用");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(requestDto.EnglishName))
            {
                ModelState.AddModelError("vm.EnglishName", "英文名称不能为空");
                return BadRequest(ModelState);
            }

            if (requestDto.PreferredName == null)
            {
                ModelState.AddModelError("vm.PreferredName", "名称语言偏好必填");
                return BadRequest(ModelState);
            }

            if (requestDto.Type == null)
            {
                ModelState.AddModelError("vm.PreferredName", "据点类型必填");
                return BadRequest(ModelState);
            }

            if (!requestDto.BackgroundImage.IsTrustedUrl())
            {
                ModelState.AddModelError("vm.BackgroundImage", "不允许使用可不信图片来源");
                return BadRequest(ModelState);
            }

            if (!requestDto.AvatarImage.IsTrustedUrl())
            {
                ModelState.AddModelError("vm.AvatarImage", "不允许使用可不信图片来源");
                return BadRequest(ModelState);
            }

            var normalPoint = DbContext.NormalPoints.Create();
            normalPoint.IdCode = requestDto.IdCode;
            normalPoint.BackgroundImage = requestDto.BackgroundImage;
            normalPoint.AvatarImage = requestDto.AvatarImage;
            normalPoint.ChineseName = requestDto.ChineseName;
            normalPoint.EnglishName = requestDto.EnglishName;
            normalPoint.PreferredName = requestDto.PreferredName.Value;
            normalPoint.ChineseAliases = requestDto.ChineseAliases;
            normalPoint.EnglishAliases = requestDto.EnglishAliases;
            normalPoint.Type = requestDto.Type.Value;
            normalPoint.Description = requestDto.Description;
            if (requestDto.Type.Value == NormalPointType.Genre ||
                requestDto.Type.Value == NormalPointType.Manufacturer)
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
                        await DbContext.SteamStoreNames.Where(n => n.Name == nameString).SingleOrDefaultAsync() ??
                        DbContext.SteamStoreNames.Create();
                    name.Name = nameString;
                    names.Add(name);
                }
                normalPoint.SteamStoreNames = names;
            }
            if (normalPoint.Type == NormalPointType.Game &&
                !await PopulateGamePointAttributes(normalPoint, requestDto, StaffClaim.Operator))
            {
                return BadRequest(ModelState);
            }
            DbContext.NormalPoints.Add(normalPoint);
            await DbContext.SaveChangesAsync();

            return Created($"normal-point/{normalPoint.Id}", new NormalPointDto(normalPoint));
        }

        /// <summary>
        ///     请求 DTO（CreateOneManually 与 UpdateOneById 公用）
        /// </summary>
        public class NormalPointCreateOrUpdateOneRequestDto
        {
            /// <summary>
            ///     背景横幅
            /// </summary>
            [Required(AllowEmptyStrings = true)]
            public string BackgroundImage { get; set; }

            /// <summary>
            ///     头像
            /// </summary>
            [Required(AllowEmptyStrings = true)]
            public string AvatarImage { get; set; }


            /// <summary>
            ///     中文名
            /// </summary>
            [Required(AllowEmptyStrings = true)]
            public string ChineseName { get; set; }

            /// <summary>
            ///     英文索引
            /// </summary>
            [Required(AllowEmptyStrings = true)]
            public string EnglishAliases { get; set; }

            /// <summary>
            ///     中文索引
            /// </summary>
            [Required(AllowEmptyStrings = true)]
            public string ChineseAliases { get; set; }

            /// <summary>
            ///     描述
            /// </summary>
            [Required(AllowEmptyStrings = true)]
            public string Description { get; set; }

            #region Admin Editable

            /// <summary>
            ///     英文名
            /// </summary>
            public string EnglishName { get; set; }

            /// <summary>
            ///     商店匹配名
            /// </summary>
            public string NameInSteamStore { get; set; }

            /// <summary>
            ///     主显名称偏好
            /// </summary>
            public PreferredNameType? PreferredName { get; set; }

            /// <summary>
            ///     识别码
            /// </summary>
            public string IdCode { get; set; }

            /// <summary>
            ///     类型
            /// </summary>
            public NormalPointType? Type { get; set; }

            #endregion

            #region Game Point Only

            /// <summary>
            ///     Steam App ID
            /// </summary>
            public int? SteamAppId { get; set; }

            /// <summary>
            ///     别名
            /// </summary>
            public string DisplayAliases { get; set; }

            /// <summary>
            ///     发行日期
            /// </summary>
            public DateTime? ReleaseDate { get; set; }

            /// <summary>
            ///     封面图片
            /// </summary>
            public string CoverImage { get; set; }

            /// <summary>
            ///     开发商据点
            /// </summary>
            public List<string> DeveloperPointsId { get; set; }

            /// <summary>
            ///     发行商据点
            /// </summary>
            public List<string> PublisherPointsId { get; set; }

            /// <summary>
            ///     流派据点
            /// </summary>
            public List<string> GenrePointsId { get; set; }

            /// <summary>
            ///     特性据点
            /// </summary>
            public List<string> TagPointsId { get; set; }

            /// <summary>
            ///     主要平台据点
            /// </summary>
            public List<string> MajorPlatformPointsId { get; set; }

            /// <summary>
            ///     次要平台据点
            /// </summary>
            public List<string> MinorPlatformPointsId { get; set; }

            /// <summary>
            ///     系列据点
            /// </summary>
            public List<string> SeriesPointsId { get; set; }

            #endregion
        }
    }
}