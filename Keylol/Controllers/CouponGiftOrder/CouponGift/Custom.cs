using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Web;
using Keylol.Models.DAL;
using Keylol.Utilities;

namespace Keylol.Controllers.CouponGiftOrder.CouponGift
{
    public class Custom : IGiftProcessor
    {
        void IGiftProcessor.GiftExchange(string userId, KeylolDbContext dbContext)
        {
            throw new BadRequestException(nameof(Custom),new Exception(Errors.Invalid));
        }
    }
}