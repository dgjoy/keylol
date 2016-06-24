namespace Keylol.Utilities
{
    /// <summary>
    ///     错误常量表
    /// </summary>
    public static class
        Errors
    {
        #region 通用错误

        /// <summary>
        ///     表示通常意义上的无效
        /// </summary>
        public const string Invalid = "invalid";

        /// <summary>
        ///     表示通常意义上的必须（不能为 null 或者不能为空）
        /// </summary>
        public const string Required = "required";

        /// <summary>
        ///     表示通常意义上的太多
        /// </summary>
        public const string TooMany = "too_many";

        /// <summary>
        ///     表示通常意义上的没有变化（已存在、重复）
        /// </summary>
        public const string Duplicate = "duplicate";

        /// <summary>
        ///     表示通常意义上的不存在
        /// </summary>
        public const string NonExistent = "non_existent";

        #endregion

        #region 特定属性错误

        /// <summary>
        ///     无效的识别码
        /// </summary>
        public const string InvalidIdCode = "invalid_id_code";

        /// <summary>
        ///     识别码已被使用
        /// </summary>
        public const string IdCodeUsed = "id_code_used";

        /// <summary>
        ///     识别码已被保留
        /// </summary>
        public const string IdCodeReserved = "id_code_reserved";

        /// <summary>
        ///     用户名长度不符合规范
        /// </summary>
        public const string UserNameInvalidLength = "user_name_invalid_length";

        /// <summary>
        ///     用户名包含无效字符
        /// </summary>
        public const string UserNameInvalidCharacter = "user_name_invalid_character";

        /// <summary>
        ///     用户名已被使用
        /// </summary>
        public const string UserNameUsed = "user_name_used";

        /// <summary>
        ///     无效的邮箱
        /// </summary>
        public const string InvalidEmail = "invalid_email";

        /// <summary>
        /// 邮箱已被使用
        /// </summary>
        public const string EmailUsed = "email_used";

        /// <summary>
        ///     玩家标签长度不符合规范
        /// </summary>
        public const string GamerTagInvalidLength = "gamer_tag_invalid_length";

        /// <summary>
        ///     密码是空白
        /// </summary>
        public const string PasswordAllWhitespace = "password_all_whitespace";

        /// <summary>
        ///     密码太短
        /// </summary>
        public const string PasswordTooShort = "password_too_short";

        /// <summary>
        ///     Steam 账户已经被绑定
        /// </summary>
        public const string SteamAccountBound = "steam_account_bound";

        /// <summary>
        ///     头像图片来源不可信
        /// </summary>
        public const string AvatarImageUntrusted = "avatar_image_untrusted";

        /// <summary>
        ///     页眉图片来源不可信
        /// </summary>
        public const string HeaderImageUntrusted = "header_image_untrusted";

        /// <summary>
        ///     全自动区分电脑与人类的图灵测试未通过
        /// </summary>
        public const string InvalidCaptcha = "invalid_captcha";

        /// <summary>
        ///     没有有效的身份标识属性
        /// </summary>
        public const string InvalidIdField = "invalid_id_field";

        /// <summary>
        ///     用户不存在
        /// </summary>
        public const string UserNonExistent = "user_non_existent";

        /// <summary>
        ///     帐号被暂时锁定
        /// </summary>
        public const string AccountLockedOut = "account_locked_out";

        /// <summary>
        ///     密码无效
        /// </summary>
        public const string InvalidPassword = "invalid_password";

        /// <summary>
        ///     无效的 Token
        /// </summary>
        public const string InvalidToken = "invalid_token";

        /// <summary>
        ///     礼品已下架
        /// </summary>
        public const string GiftOffTheMarket = "gift_off_the_market";

        /// <summary>
        ///     已经兑换过礼品
        /// </summary>
        public const string GiftOwned = "gift_owned";

        /// <summary>
        ///     文券不足
        /// </summary>
        public const string NotEnoughCoupon = "not_enough_coupon";

        /// <summary>
        /// 网络错误
        /// </summary>
        public const string NetworkError = "network_error";

        /// <summary>
        ///     不支持的 Steam 应用（不是游戏或硬件）
        /// </summary>
        public const string SteamAppNotSupported = "steam_app_not_supported";

        /// <summary>
        ///     不支持 Steam 上的 DLC
        /// </summary>
        public const string SteamDlcNotSupported = "steam_dlc_not_supported";

        /// <summary>
        /// 无效的主题色
        /// </summary>
        public const string InvalidThemeColor = "invalid_theme_color";

        /// <summary>
        /// 无效的轻主题色
        /// </summary>
        public const string InvalidLightThemeColor = "invalid_light_theme_color";

        #endregion
    }
}