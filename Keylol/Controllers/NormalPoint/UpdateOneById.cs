using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.ViewModels;
using Keylol.Provider;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在")]
        public async Task<IHttpActionResult> UpdateOneById(string id, NormalPointVM vm)
        {
            var normalPoint = await DbContext.NormalPoints
                .Include(p => p.DeveloperPoints)
                .Include(p => p.PublisherPoints)
                .Include(p => p.SeriesPoints)
                .Include(p => p.GenrePoints)
                .Include(p => p.TagPoints)
                .Include(p => p.MajorPlatformPoints)
                .Include(p => p.MinorPlatformPoints)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (normalPoint == null)
                return NotFound();

            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!Regex.IsMatch(vm.IdCode, @"^[A-Z0-9]{5}$"))
            {
                ModelState.AddModelError("vm.IdCode", "识别码只允许使用 5 位数字或大写字母");
                return BadRequest(ModelState);
            }
            if (vm.IdCode != normalPoint.IdCode &&
                await DbContext.NormalPoints.SingleOrDefaultAsync(u => u.IdCode == vm.IdCode) != null)
            {
                ModelState.AddModelError("vm.IdCode", "识别码已经被其他据点使用");
                return BadRequest(ModelState);
            }
            if (!vm.BackgroundImage.IsTrustedUrl())
            {
                ModelState.AddModelError("vm.BackgroundImage", "不允许使用可不信图片来源");
                return BadRequest(ModelState);
            }
            if (!vm.AvatarImage.IsTrustedUrl())
            {
                ModelState.AddModelError("vm.AvatarImage", "不允许使用可不信图片来源");
                return BadRequest(ModelState);
            }
            var editorStaffClaim = await UserManager.GetStaffClaimAsync(User.Identity.GetUserId());
            if (editorStaffClaim == StaffClaim.Operator)
            {
                normalPoint.EnglishName = vm.EnglishName;
                normalPoint.IdCode = vm.IdCode;
                normalPoint.PreferredName = vm.PreferredName;
                if (normalPoint.Type == NormalPointType.Genre || normalPoint.Type == NormalPointType.Manufacturer)
                    normalPoint.NameInSteamStore = vm.NameInSteamStore;
            }
            normalPoint.BackgroundImage = vm.BackgroundImage;
            normalPoint.AvatarImage = vm.AvatarImage;
            normalPoint.ChineseName = vm.ChineseName;
            normalPoint.ChineseAliases = vm.ChineseAliases;
            normalPoint.EnglishAliases = vm.EnglishAliases;
            normalPoint.Description = vm.Description;
            if (normalPoint.Type == NormalPointType.Game &&
                !await PopulateGamePointAttributes(normalPoint, vm, editorStaffClaim, true))
            {
                return BadRequest(ModelState);
            }
            await DbContext.SaveChangesAsync();
            await RedisProvider.Delete($"point:{normalPoint.Id}:related");
            return Ok();
        }
    }
}