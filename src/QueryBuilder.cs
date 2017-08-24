using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;

namespace QueryBuilder
{
    public class QueryBuilder : IDisposable
    {
        // Backing query stores
        private DynamicParameters _params;
        private int _paramCount = 0;
        private Dictionary<object, string> _paramMap = new Dictionary<object, string>();

        public QueryBuilder()
        {
            _params = new DynamicParameters();
        }

        public string Build(FormattableString sql)
        {
            // Get all interpolated values and parameterize them
            var pArgs = new List<string>();
            foreach (var argument in sql.GetArguments())
            {
                switch (argument)
                {
                    case IList arguments:
                        pArgs.Add(Parameterize(arguments));
                        break;
                    default:
                        pArgs.Add(Parameterize(argument));
                        break;
                }
            }

            // Automatically handle parameterizing interpolated strings
            return string.Format(sql.Format, pArgs.ToArray());
        }

        private string Parameterize(object parameter)
        {
            if (_paramMap.ContainsKey(parameter))
            {
                // If it exists, just use the same parameter
                return _paramMap[parameter];
            }

            var parameterName = $"@P{++_paramCount}";
            _params.Add(parameterName, parameter);
            _paramMap.Add(parameter, parameterName);

            return parameterName;
        }

        private string Parameterize(IList parameters)
        {
            var paramNames = new List<string>();
            foreach (var parameter in parameters)
            {
                paramNames.Add(Parameterize(parameter));
            }

            return string.Join(",", paramNames);
        }

        #region IDisposable Support

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _params = null;
                    _paramCount = 0;
                    _paramMap = null;
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
