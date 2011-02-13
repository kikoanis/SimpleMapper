using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper
{
	public class SimpleMapper<TSource, TDestination>
		where TSource : class, new()
		where TDestination : class, new()
	{
		private readonly IDictionary<string, MemberInfo> _propertyMapping = new Dictionary<string, MemberInfo>();
		private readonly IDictionary<string, MemberInfo> _excludedProperties = new Dictionary<string, MemberInfo>();
		private Expression<Func<TDestination, object>> _destinationProperty;
		private TDestination _existingDestination;

		///<summary>
		/// Maps a destination property to source property that might not carry the same name
		///</summary>
		///<param name="sourceProperty"></param>
		///<returns></returns>
		public SimpleMapper<TSource, TDestination> AssignProperty(Expression<Func<TSource, object>> sourceProperty)
		{

			if (sourceProperty == null || _destinationProperty == null) return this;
			var dest = FindPropertyName(_destinationProperty);
			var source = FindPropertyName(sourceProperty);
			if (!_propertyMapping.ContainsKey(source.Name))
			{
				_propertyMapping.Add(new KeyValuePair<string, MemberInfo>(source.Name, dest));
			}
			_destinationProperty = null;
			return this;

		}

		///<summary>
		/// Maps a destination property to source property that might not carry the same name
		///</summary>
		///<param name="destinationProperty"></param>
		///<returns></returns>
		public SimpleMapper<TSource, TDestination> ForProperty(
			Expression<Func<TDestination, object>> destinationProperty)
		{
			if (destinationProperty == null) return this;
			_destinationProperty = destinationProperty;
			return this;
		}

		/// <summary>
		/// Uses an existing destination object to map source to
		/// </summary>
		/// <param name="destination"></param>
		/// <returns></returns>
		public SimpleMapper<TSource, TDestination> UseExisting(TDestination destination)
		{
			_existingDestination = destination;
			return this;
		}

		/// <summary>
		/// Excludes a property from mapping
		/// </summary>
		/// <param name="sourceProperty"></param>
		/// <returns></returns>
		public SimpleMapper<TSource, TDestination> Exclude(Expression<Func<TSource, object>> sourceProperty)
		{
			if (sourceProperty == null) return this;
			var source = FindPropertyName(sourceProperty);
			if (!_excludedProperties.ContainsKey(source.Name))
			{
				_excludedProperties.Add(new KeyValuePair<string, MemberInfo>(source.Name, source));
			}
			return this;
		}

		///<summary>
		/// Maps from source object to destination object
		///</summary>
		///<returns></returns>
		public TDestination Map(TSource source)
		{
			var destination = _existingDestination ?? new TDestination();
			if (source == null) return destination;
			var sourceType = typeof(TSource);
			var destinationType = typeof(TDestination);
			var sourceProperties = sourceType.GetProperties();
			foreach (var sourceProperty in sourceProperties)
			{
				if (_excludedProperties.ContainsKey(sourceProperty.Name)) continue;
				var sourceValue = sourceType.GetProperty(sourceProperty.Name).GetValue(source, null);
				if (_propertyMapping.ContainsKey(sourceProperty.Name))
				{
					var destinationPropertyInfo = _propertyMapping[sourceProperty.Name] as PropertyInfo;

					if (destinationPropertyInfo != null &&
						sourceProperty.PropertyType == destinationPropertyInfo.PropertyType)
					{
						var destProperty = destinationType.GetProperty(destinationPropertyInfo.Name);
						destProperty.SetValue(destination, sourceValue, null);
					}
				}
				else
				{
					var destinationProperty = destinationType.GetProperty(sourceProperty.Name);
					if (destinationProperty != null && destinationProperty.PropertyType == sourceProperty.PropertyType)
					{
						destinationProperty.SetValue(destination, sourceValue, null);
					}
				}
			}
			return destination;
		}

		private static MemberInfo FindPropertyName(Expression expression)
		{
			var notDone = true;
			while (notDone)
			{
				switch (expression.NodeType)
				{
					case ExpressionType.MemberAccess:
						var memberExpression = ((MemberExpression)expression);

						if (memberExpression.Expression.NodeType != ExpressionType.Parameter &&
							memberExpression.Expression.NodeType != ExpressionType.Convert)
						{
							throw new ArgumentException(string.Format("Expression \"{0}\" cannot be resolved", expression));
						}

						var member = memberExpression.Member;

						return member;
					case ExpressionType.Lambda:
						expression = ((LambdaExpression)expression).Body;
						break;
					case ExpressionType.Convert:
						expression = ((UnaryExpression)expression).Operand;
						break;
					default:
						notDone = false;
						break;
				}
			}
			return null;
		}
	}
}
