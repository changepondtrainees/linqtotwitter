﻿/***********************************************************
 * Credits:
 * 
 * MSDN Documentation -
 * Walkthrough: Creating an IQueryable LINQ Provider
 * 
 * http://msdn.microsoft.com/en-us/library/bb546158.aspx
 * 
 * Matt Warren's Blog -
 * LINQ: Building an IQueryable Provider:
 * 
 * http://blogs.msdn.com/mattwar/default.aspx
 * 
 * Modified By: Joe Mayo, 8/26/08
 * 
 * Added constructor to pass TwitterContext to Provider
 * *********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
#if NETFX_CORE
using System.Reflection;
#endif

namespace LinqToTwitter
{
    /// <summary>
    /// IQueryable of T part of LINQ to Twitter
    /// </summary>
    /// <typeparam name="T">Type to operate on</typeparam>
    public class TwitterQueryable<T> : IOrderedQueryable<T>
    {
        /// <summary>
        /// init with TwitterContext
        /// </summary>
        /// <param name="context"></param>
        public TwitterQueryable(TwitterContext context)
        {
            Provider = new TwitterQueryProvider();
            Expression = Expression.Constant(this);

            // lets provider reach back to TwitterContext, 
            // where execute implementation resides
            ((TwitterQueryProvider) Provider).Context = context;
        }

        /// <summary>
        /// modified as internal because LINQ to Twitter is Unusable 
        /// without TwitterContext, but provider still needs access
        /// </summary>
        /// <param name="provider">IQueryProvider</param>
        /// <param name="expression">Expression Tree</param>
        internal TwitterQueryable(
            TwitterQueryProvider provider,
            Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

#if NETFX_CORE
            if (!typeof(IQueryable<T>).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
            {
                throw new ArgumentOutOfRangeException("expression");
            }
#else
            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }
#endif

            Provider = provider;
            Expression = expression;
        }

        /// <summary>
        /// IQueryProvider part of LINQ to Twitter
        /// </summary>
        public IQueryProvider Provider { get; private set; }
        
        /// <summary>
        /// expression tree
        /// </summary>
        public Expression Expression { get; private set; }

        /// <summary>
        /// type of T in IQueryable of T
        /// </summary>
        public Type ElementType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// executes when iterating over collection
        /// </summary>
        /// <returns>query results</returns>
        public IEnumerator<T> GetEnumerator()
        {
            var enumerator = (Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
            return enumerator;
        }

        /// <summary>
        /// non-generic execution when collection is iterated over
        /// </summary>
        /// <returns>query results</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (Provider.Execute<IEnumerable>(Expression)).GetEnumerator();
        }
    }
}
