using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models.ViewModels;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        ///     修改用户设置
        /// </summary>
        /// <param name="id">用户 ID</param>
        /// <param name="vm">用户相关属性</param>
        [Route("{id}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前登录用户无权编辑指定用户")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> UpdateOneById(string id, UserPutVM vm)
        {
            if (User.Identity.GetUserId() != id)
                return Unauthorized();

            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UserManager.FindByIdAsync(id);

            if (vm.NewPassword != null || vm.LockoutEnabled != null)
            {
                if (vm.Password == null)
                {
                    ModelState.AddModelError("vm.Password", "Password cannot be empty.");
                    return BadRequest(ModelState);
                }

                var geetest = new Geetest();
                if (vm.GeetestChallenge == null || vm.GeetestSeccode == null || vm.GeetestValidate == null ||
                    !await geetest.ValidateAsync(vm.GeetestChallenge, vm.GeetestSeccode, vm.GeetestValidate))
                {
                    ModelState.AddModelError("authCode", "true");
                    return BadRequest(ModelState);
                }

                if (vm.NewPassword != null)
                {
                    var resultPassword = await UserManager.ChangePasswordAsync(id, vm.Password, vm.NewPassword);
                    if (!resultPassword.Succeeded)
                    {
                        foreach (var error in resultPassword.Errors)
                        {
                            if (error.Contains("Incorrect password"))
                                ModelState.AddModelError("vm.Password", "Password is not correct.");
                            else
                                ModelState.AddModelError("vm.NewPassword", error);
                        }
                        return BadRequest(ModelState);
                    }
                }
                else
                {
                    if (!await UserManager.CheckPasswordAsync(user, vm.Password))
                    {
                        ModelState.AddModelError("vm.Password", "Password is not correct.");
                        return BadRequest(ModelState);
                    }
                }
            }

            vm.CopyToUser(user);
            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Contains("Email"))
                        ModelState.AddModelError("vm.Email", error);
                    else if (error.Contains("GamerTag"))
                        ModelState.AddModelError("vm.GamerTag", error);
                }
                return BadRequest(ModelState);
            }
            return Ok();
        }
    }
}