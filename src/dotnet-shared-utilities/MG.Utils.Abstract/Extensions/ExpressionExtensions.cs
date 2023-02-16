using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MG.Utils.Abstract.Extensions
{
    // https://habr.com/ru/post/313394/
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        // https://habr.com/ru/post/313394/
        public static IQueryable<T> Where<T, TParam>(
            this IQueryable<T> queryable,
            Expression<Func<T, TParam>> prop,
            Expression<Func<TParam, bool>> where)
        {
            return queryable.Where(prop.ComposeWhere(where));
        }

        // https://habr.com/ru/post/313394/
        public static Expression<Func<TIn, TOut>> ComposeWhere<TIn, TInOut, TOut>(
            this Expression<Func<TIn, TInOut>> input,
            Expression<Func<TInOut, TOut>> inOutOut)
        {
            var param = Expression.Parameter(typeof(TIn), null);
            var invoke = Expression.Invoke(input, param);
            var res = Expression.Invoke(inOutOut, invoke);

            return Expression.Lambda<Func<TIn, TOut>>(res, param);
        }

        private static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // zip parameters (map from parameters of second to parameters of first)
            var map = first.Parameters
                .Select((f, i) => new { f, s = second.Parameters[i] })
                .ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with the parameters in the first
            var secondBody = ParameterRebind.ReplaceParameters(map, second.Body);

            // create a merged lambda expression with parameters from the first expression
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        private class ParameterRebind : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

            private ParameterRebind(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            {
                return new ParameterRebind(map).Visit(exp);
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                if (_map.TryGetValue(p, out ParameterExpression replacement))
                {
                    p = replacement;
                }

                return base.VisitParameter(p);
            }
        }
    }
}