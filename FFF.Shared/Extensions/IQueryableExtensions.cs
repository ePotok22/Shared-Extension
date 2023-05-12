using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FFF.Shared
{
    public static class IQueryableExtensions
    {
        //used by LINQ to SQL
        public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, int page, int pageSize) =>
            source.Skip((page - 1) * pageSize).Take(pageSize);

        /// <summary>
        /// Adds ability to order by text value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByText<T>(this IQueryable<T> source, string property)
        {
            if (string.IsNullOrEmpty(property))
                throw new ArgumentNullException("property");

            return ApplyOrder<T>(source, property, "OrderBy");
        }

        /// <summary>
        /// this method creates the expression and the uses
        /// reflection to construct a method call
        /// breaking out into an second method allows us to create variations
        /// that can use other methods such as OrderByDescending
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">Source queryable of T</param>
        /// <param name="property"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection to get meta data for
                // the object we wish to sort by
                PropertyInfo pi = type.GetRuntimeProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            //Create the Lambda expression
            Type delegateType =
              typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda =
              Expression.Lambda(delegateType, expr, arg);

            // use reflection to call the sort method using the
            // Lambda expression
            object result = typeof(Queryable).GetRuntimeMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), type)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }
    }
}
