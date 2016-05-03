using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Keylol.Utilities
{
    /// <summary>
    /// 断言构造
    /// </summary>
    public static class PredicateBuilder
    {
        /// <summary>
        /// 强制类型转换
        /// </summary>
        /// <param name="value">原始值</param>
        /// <param name="targetType">目标类型示例对象</param>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>转换为目标类型的对象</returns>
        public static T CastTo<T>(this object value, T targetType)
        {
            return (T) value;
        }

        /// <summary>
        /// 包含关系
        /// </summary>
        public static Expression<Func<TElement, bool>> Contains<TElement, TValue>(IEnumerable<TValue> values,
            Expression<Func<TElement, TValue>> valueSelector, TElement typeHint = default(TElement))
        {
            if (null == valueSelector)
                throw new ArgumentNullException(nameof(valueSelector));
            if (null == values)
                throw new ArgumentNullException(nameof(values));
            var p = valueSelector.Parameters.Single();

            var enumerable = values as IList<TValue> ?? values.ToList();
            if (!enumerable.Any())
                return e => false;

            var equals = enumerable.Select(value =>
                (Expression) Expression.Equal(
                    valueSelector.Body,
                    Expression.Constant(
                        value,
                        typeof(TValue)))
                );
            var body = equals.Aggregate(Expression.Or);
            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }

        /// <summary>
        /// 真
        /// </summary>
        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }

        /// <summary>
        /// 假
        /// </summary>
        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        /// <summary>
        /// 或
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>>
                (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        /// <summary>
        /// 与
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>>
                (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}