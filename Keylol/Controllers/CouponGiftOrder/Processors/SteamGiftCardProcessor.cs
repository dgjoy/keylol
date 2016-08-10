using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.CouponGiftOrder.Processors
{
    /// <summary>
    /// Steam 礼品卡
    /// </summary>
    public class SteamGiftCardProcessor : GiftProcessor
    {
        private readonly KeylolDbContext _dbContext;
        private readonly CouponProvider _coupon;
        private const int CreditBase = 0;

        /// <summary>
        /// 创建 <see cref="SteamGiftCardProcessor"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        public SteamGiftCardProcessor(KeylolDbContext dbContext, CouponProvider coupon)
        {
            _dbContext = dbContext;
            _coupon = coupon;
        }

        /// <summary>
        /// 商品兑换
        /// </summary>
        public override async Task RedeemAsync()
        {
            if (await GetCreditAsync() < Gift.Price)
                throw new Exception(Errors.NotEnoughCredit);

            if (string.IsNullOrWhiteSpace(User.Email))
                throw new Exception(Errors.EmailNonExistent);

            var order = new Models.CouponGiftOrder
            {
                UserId = User.Id,
                GiftId = Gift.Id,
                RedeemPrice = Gift.Price
            };
            _dbContext.CouponGiftOrders.Add(order);
            await _dbContext.SaveChangesAsync();
            await _coupon.UpdateAsync(User, CouponEvent.兑换商品, -Gift.Price, new {CouponGiftId = Gift.Id});

            var userManager = Global.Container.GetInstance<KeylolUserManager>();
            await userManager.SendEmailAsync(User.Id, "你的 Steam 钱包卡兑换订单", GenerateEmail(order));
            await userManager.SendSteamChatMessageAsync(User,
                $"你的 Steam 钱包卡兑换订单已生成，详细信息已发送到你登记的邮箱 ({User.Email}) 中。若五分钟内没有收到任何邮件，请检查你的垃圾邮件文件夹，或与我们的站务职员邮箱 (alpha.feedback@keylol.com) 联络。");
            await userManager.EmailService.SendAsync(new IdentityMessage
            {
                Destination = "lee@keylol.com; stackia@keylol.com",
                Subject = $"用户兑换 Steam 充值卡通知 - {User.UserName}",
                Body = $@"<p>订单编号：{order.Id}<br />
UIC：{User.IdCode}<br />
用户名：{User.UserName}<br />
Email：{User.Email}<br />
兑换时间：{order.RedeemTime.ToString("yyyy-MM-dd HH:mm:ss")}<br />
面值：¥{order.RedeemPrice} CNY<br />
价格：{order.RedeemPrice} ◆</p>"
            });
        }

        /// <summary>
        /// 填充状态树对象的属性
        /// </summary>
        /// <param name="stateTreeGift">状态树商品对象</param>
        public override async Task FillPropertiesAsync(States.Coupon.Store.CouponGift stateTreeGift)
        {
            stateTreeGift.Email = User.Email;
            stateTreeGift.Credit = await GetCreditAsync();
            stateTreeGift.Value = Gift.Value;
        }

        private async Task<int> GetCreditAsync()
        {
            var boughtCredits = await _dbContext.CouponGiftOrders.Where(giftOrder =>
                giftOrder.RedeemTime.Month == DateTime.Now.Month && giftOrder.UserId == User.Id &&
                giftOrder.Gift.Type == Gift.Type).Select(o => o.Gift.Value).DefaultIfEmpty(0).SumAsync();

            return CreditBase + User.SeasonLikeCount - boughtCredits;
        }

        private string GenerateEmail(Models.CouponGiftOrder order)
        {
            return $@"<p>{User.UserName},</p>
<p>　　感谢你对其乐社区质量的认可与贡献！</p>
<p>　　你在文券商店兑换的 Steam 钱包卡已成功生成订单。出于对社区内容作者的尊重，我们会对所有兑换者的文券历史进行简单的来源审核，并会在 96 小时以内通过电邮将充值代码发送到当前的电邮地址。请妥善保管以下订单信息以便日后问询。</p>
<p>订单编号：{order.Id.ToUpper()}<br />
UIC：{User.IdCode}<br />
兑换时间：{order.RedeemTime.ToString("yyyy-MM-dd HH:mm:ss")}<br />
面值：¥{Gift.Value} CNY<br />
价格：{order.RedeemPrice} ◆<br />
预计发码时间：{(order.RedeemTime + TimeSpan.FromDays(3)).ToString("yyyy-MM-dd HH:mm:ss")}</p>
<p>　　请不要回复此则电邮，如有任何联系我们的需要，请致信其乐的反馈邮箱 <a href=""mailto:alpha.feedback@keylol.com"">alpha.feedback@keylol.com</a>，如果你在垃圾邮件文件夹中找到了当前这封邮件，请记得将我们的发件人地址（postman@noreply.keylol.com）加入过滤白名单，以免错过之后的兑换码。</p>
<p>蒸汽动力 其乐游戏社区</p>
<p>Keylol.com</p>";
        }
    }
}