using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FFF.Shared
{
    public static class ExpressionExtensions
    {/// <summary>
     /// Allows you to build an OR expression
     /// </summary>
     /// <typeparam name="TElement"></typeparam>
     /// <typeparam name="TValue"></typeparam>
     /// <param name="valueSelector"></param>
     /// <param name="values"></param>
     /// <returns></returns>
        public static Expression<Func<TElement, bool>> BuildOrExpression<TElement, TValue>(Expression<Func<TElement, TValue>> valueSelector, IEnumerable<TValue> values)
        {
            if (null == valueSelector)
                throw new ArgumentNullException("valueSelector");
            if (null == values)
                throw new ArgumentNullException("values");
            ParameterExpression p = valueSelector.Parameters.Single();

            if (!values.Any())
                return e => false;

            var equals = values.Select(value =>
                (Expression)Expression.Equal(
                     valueSelector.Body,
                     Expression.Constant(
                         value,
                         typeof(TValue)
                     )
                )
            );
            Expression body = equals.Aggregate((accumulate, equal) => Expression.Or(accumulate, equal));

            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }
    }
}
