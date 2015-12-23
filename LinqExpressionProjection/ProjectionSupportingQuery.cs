﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;

namespace LinqExpressionProjection
{
	/// <summary>
	/// An IQueryable wrapper that allows us to visit the query's expression tree just before LINQ to SQL gets to it.
	/// This is based on the excellent work of Tomas Petricek: http://tomasp.net/blog/linq-expand.aspx
	/// </summary>
	public class ProjectionSupportingQuery<T> : IQueryable<T>, IOrderedQueryable<T>, IOrderedQueryable
    {
        ProjectionSupportingQueryProvider<T> _provider;
		IQueryable<T> _inner;

		internal IQueryable<T> InnerQuery { get { return _inner; } }			// Original query, that we're wrapping

        internal ProjectionSupportingQuery(IQueryable<T> inner)
		{
			_inner = inner;
            _provider = new ProjectionSupportingQueryProvider<T>(this);
		}

		Expression IQueryable.Expression { get { return _inner.Expression; } }
		Type IQueryable.ElementType { get { return typeof (T); } }
		IQueryProvider IQueryable.Provider { get { return new AsyncQueryProviderWrapper(_provider); } }
		public IEnumerator<T> GetEnumerator () { return _inner.GetEnumerator (); }
		IEnumerator IEnumerable.GetEnumerator () { return _inner.GetEnumerator (); }
		public override string ToString () { return _inner.ToString (); }
    }

    class ProjectionSupportingQueryProvider<T> : IQueryProvider
	{
        ProjectionSupportingQuery<T> _query;

        internal ProjectionSupportingQueryProvider(ProjectionSupportingQuery<T> query)
		{
			_query = query;
		}

		// The following four methods first call ExpressionExpander to visit the expression tree, then call
		// upon the inner query to do the remaining work.

		IQueryable<TElement> IQueryProvider.CreateQuery<TElement> (Expression expression)
		{
            return new ProjectionSupportingQuery<TElement>(_query.InnerQuery.Provider.CreateQuery<TElement>(expression.ExpandExpressionsForProjection()));
		}

		IQueryable IQueryProvider.CreateQuery (Expression expression)
		{
			return _query.InnerQuery.Provider.CreateQuery (expression.ExpandExpressionsForProjection());
		}

		TResult IQueryProvider.Execute<TResult> (Expression expression)
		{
            return _query.InnerQuery.Provider.Execute<TResult>(expression.ExpandExpressionsForProjection());
		}

		object IQueryProvider.Execute (Expression expression)
		{
            return _query.InnerQuery.Provider.Execute(expression.ExpandExpressionsForProjection());
		}
	}
}