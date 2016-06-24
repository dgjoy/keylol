using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Results;
using Keylol.Models;
using Keylol.Services;

namespace Keylol.Utilities
{
    /// <summary>
    ///     一些常用对象扩展
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     转换为 Unix 时间戳（秒）形式
        /// </summary>
        /// <param name="dateTime">要转换的 DateTime 对象</param>
        /// <returns>Unix 时间戳（秒）</returns>
        public static long ToTimestamp(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000)/10000000;
        }

        /// <summary>
        ///     从一个集合中取出指定数量元素的所以组合情况
        /// </summary>
        /// <param name="items">集合</param>
        /// <param name="count">取出元素数量</param>
        /// <typeparam name="T">集合类型</typeparam>
        /// <returns>全部组合</returns>
        public static IEnumerable<IEnumerable<T>> AllCombinations<T>(this IEnumerable<T> items, int count)
        {
            var i = 0;
            var list = items as IList<T> ?? items.ToList();
            foreach (var item in list)
            {
                if (count == 1)
                    yield return new[] {item};
                else
                {
                    foreach (var result in list.Skip(i + 1).AllCombinations(count - 1))
                        yield return new[] {item}.Concat(result);
                }
                i++;
            }
        }

        /// <summary>
        ///     将字符串转换为指定的 Enum 类型
        /// </summary>
        /// <param name="text">要转换的字符串</param>
        /// <typeparam name="TEnum">转换目标 Enum 类型</typeparam>
        /// <returns>如果转换成功，返回对应 Enum 值，如果失败，返回该 Enum 类型默认值</returns>
        public static TEnum ToEnum<TEnum>(this string text) where TEnum : struct
        {
            TEnum result;
            if (Enum.TryParse(text, out result) && Enum.IsDefined(typeof(TEnum), result))
                return result;
            return default(TEnum);
        }

        /// <summary>
        ///     判断 Steam 机器人是否属于“在线”状态
        /// </summary>
        /// <param name="bot">Steam 机器人实体</param>
        /// <returns>是否在线</returns>
        public static bool IsOnline(this SteamBot bot)
        {
            return bot.Online && bot.SessionId != null &&
                   SteamBotCoordinator.Sessions.ContainsKey(bot.SessionId);
        }

        /// <summary>
        ///     为 ModelState 增加指定错误并返回 BadRequest
        /// </summary>
        /// <param name="controller"><see cref="ApiController" /> 对象</param>
        /// <param name="modelError">错误描述，最后一个出现的字符串将作为 errorMessage，其他字符串用 "." 拼接后作为 key</param>
        /// <returns><see cref="IHttpActionResult" /> 对象</returns>
        public static IHttpActionResult BadRequest(this ApiController controller, params string[] modelError)
        {
            controller.ModelState.AddModelError(string.Join(".", modelError.Take(modelError.Length - 1)),
                modelError.Last());
            return new InvalidModelStateResult(controller.ModelState, controller);
        }

        /// <summary>
        /// 转换命名风格
        /// </summary>
        /// <param name="text">要转换的文本</param>
        /// <param name="sourceCase">原始格式</param>
        /// <param name="targetCase">目标格式</param>
        /// <returns></returns>
        public static string ToCase(this string text, NameConventionCase sourceCase, NameConventionCase targetCase)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            var breakpoints = new List<int> {0};
            for (var i = 1; i < text.Length; i++)
            {
                if (char.IsSurrogate(text[i]))
                    continue;
                switch (sourceCase)
                {
                    case NameConventionCase.PascalCase:
                    case NameConventionCase.CamelCase:
                        if (char.IsUpper(text[i]) && (char.IsLower(text[i - 1]) ||
                                                      (i != text.Length - 1 && char.IsLower(text[i + 1]))))
                            breakpoints.Add(i);
                        break;

                    case NameConventionCase.DashedCase:
                        if (text[i - 1] == '-')
                            breakpoints.Add(i);
                        break;

                    case NameConventionCase.SnakeCase:
                        if (text[i - 1] == '_')
                            breakpoints.Add(i);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sourceCase), sourceCase, null);
                }
            }
            breakpoints.Add(text.Length);

            var finalBreakpoints = new List<int>(breakpoints.Count) {0};
            int minGap;
            string replaceChar = null;
            switch (sourceCase)
            {
                case NameConventionCase.PascalCase:
                    minGap = 1;
                    break;

                case NameConventionCase.CamelCase:
                    minGap = 1;
                    break;

                case NameConventionCase.DashedCase:
                    minGap = 2;
                    replaceChar = "-";
                    break;

                case NameConventionCase.SnakeCase:
                    minGap = 2;
                    replaceChar = "_";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetCase), targetCase, null);
            }
            for (var i = 1; i < breakpoints.Count - 1; i++)
            {
                if (breakpoints[i] - breakpoints[i - 1] <= minGap && (breakpoints[i + 1] - breakpoints[i] <= minGap))
                    continue;
                finalBreakpoints.Add(breakpoints[i]);
            }
            finalBreakpoints.Add(text.Length);

            var resultBuilder = new StringBuilder(text.Length);
            for (var i = 0; i < finalBreakpoints.Count - 1; i++)
            {
                var word = text.Substring(finalBreakpoints[i], finalBreakpoints[i + 1] - finalBreakpoints[i]);
                word = replaceChar == null ? word : word.Replace(replaceChar, string.Empty);
                if (string.IsNullOrEmpty(word))
                    continue;
                switch (targetCase)
                {
                    case NameConventionCase.PascalCase:
                        resultBuilder.Append(char.ToUpper(word[0]));
                        resultBuilder.Append(word.Substring(1).ToLower());
                        break;

                    case NameConventionCase.CamelCase:
                        if (i == 0)
                        {
                            resultBuilder.Append(word.ToLower());
                        }
                        else
                        {
                            resultBuilder.Append(char.ToUpper(word[0]));
                            resultBuilder.Append(word.Substring(1).ToLower());
                        }
                        break;

                    case NameConventionCase.DashedCase:
                        resultBuilder.Append(word.ToLower());
                        if (i < finalBreakpoints.Count - 2)
                            resultBuilder.Append('-');
                        break;

                    case NameConventionCase.SnakeCase:
                        resultBuilder.Append(word.ToLower());
                        if (i < finalBreakpoints.Count - 2)
                            resultBuilder.Append('_');
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetCase), targetCase, null);
                }
            }
            return resultBuilder.ToString();
        }

        /// <summary>
        /// 获取指定分页的记录
        /// </summary>
        /// <param name="source">查询源</param>
        /// <param name="page">分页页码</param>
        /// <param name="recordsPerPage">每页记录数</param>
        /// <typeparam name="TSource">记录类型</typeparam>
        public static IQueryable<TSource> TakePage<TSource>(this IQueryable<TSource> source, int page,
            int recordsPerPage)
        {
            if (page <= 0)
                throw new ArgumentException("Page must be greater than zero.", nameof(page));
            if (recordsPerPage <= 0)
                throw new ArgumentException("Records per page must be greater than zero.", nameof(page));

            var skip = recordsPerPage*(page - 1);
            return source.Skip(() => skip).Take(() => recordsPerPage);
        }
    }

    /// <summary>
    /// 常见命名约定格式
    /// </summary>
    public enum NameConventionCase
    {
        /// <summary>
        /// PascalCase
        /// </summary>
        PascalCase,

        /// <summary>
        /// camelCase
        /// </summary>
        CamelCase,

        /// <summary>
        /// dashed-case
        /// </summary>
        DashedCase,

        /// <summary>
        /// snake_case
        /// </summary>
        SnakeCase
    }
}