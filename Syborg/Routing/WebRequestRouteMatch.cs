using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.RegularExpressions;
using Syborg.Common;

namespace Syborg.Routing
{
    public class WebRequestRouteMatch
    {
        public static WebRequestRouteMatch FromRegexMatch(Regex pattern, Match match)
        {
            Contract.Requires(pattern != null);

            WebRequestRouteMatch routeMatch = new WebRequestRouteMatch();

            foreach (string groupName in pattern.GetGroupNames())
            {
                Group group = match.Groups[groupName];

                if (@group.Captures.Count > 1)
                    throw new NotImplementedException ("group.Captures.Count > 1");

                if (!@group.Success)
                    routeMatch.AddParameter(groupName, null);
                else
                    routeMatch.AddParameter(groupName, @group.Captures[0].Value);
            }

            return routeMatch;
        }

        public void AddParameter(string name, string value)
        {
            Contract.Requires(name != null);

            parameters.Add(name, value);
        }

        public string this[string parameterName]
        {
            get
            {
                Contract.Requires(parameterName != null);

                string value;
                parameters.TryGetValue(parameterName, out value);
                return value;
            }
        }

        public T GetMandatoryParameter<T>(string parameterName)
        {
            Contract.Requires(parameterName != null);

            string value;
            if (!parameters.TryGetValue (parameterName, out value))
                throw new RouteParameterException("Route parameter '{0}' is missing".Fmt(parameterName));

            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new RouteParameterException ("Route parameter '{0}' has invalid value '{1}'".Fmt (parameterName, value), ex);
            }
        }

        public override string ToString ()
        {
            return parameters.Concat(x => "{0}={1}".Fmt(x.Key, x.Value), ", ");
        }

        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();
    }
}