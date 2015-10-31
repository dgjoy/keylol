using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Keylol.Utilities
{
    public static class PredicateBuilder
    {
        public static T CastTo<T>(this object value, T targetType)
        {
            return (T)value;
        }

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
                        typeof (TValue)))
                );
            var body = equals.Aggregate(Expression.Or);
            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }

        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }

        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}