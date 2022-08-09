﻿using System.Reflection;
using System.Text;

namespace Repository.Extensions.Utility;

public class OrderQueryBuilder
{
    public static string CreateOrderQuery<T>(string orderByQueryString)
    {
        var orderParams = orderByQueryString.Trim().Split(',');
        var propertyInfos = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var orderQueryBuilder = new StringBuilder(38);

        foreach (var param in orderParams)
        {
            if (string.IsNullOrWhiteSpace(param))
                continue;

            var propertyFromQueryName = param.Split(" ")[0];
            PropertyInfo? objectProperty = propertyInfos.FirstOrDefault(pi =>
                pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

            if (objectProperty == null)
                continue;

            var direction = param.EndsWith(" desc") ? "descending" : "ascending";

            orderQueryBuilder.Append($"{objectProperty.Name} {direction}, ");
        }

        var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');

        return orderQuery;
    }
}