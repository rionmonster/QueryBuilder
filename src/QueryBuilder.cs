using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace QueryBuilder
{
    public class QueryBuilder : IDisposable
    {
        public string CurrentQuery => _stringBuilder.ToString();

        // Storage for the current query
        private StringBuilder _stringBuilder = new StringBuilder();

        // Question: Is this needed right now? Would a Dictionary alone be sufficient?
        private DynamicParameters _params = new DynamicParameters();
        // Dictionary to keep track of what parameters we've seen and their respective parameters
        private Dictionary<object, string> _paramMap = new Dictionary<object, string>();
        // Counter for parameter naming purposes
        private int _paramCount = 0;

        public QueryBuilder()
        {
            
        }

        // TODO: Add methods to retrieve SqlCommand and DapperParameters objects
        //       These may / may not be stored internally, who knows.
        public SqlCommand GetSqlCommand()
        {
            var sqlCommand = new SqlCommand(CurrentQuery);
            foreach (var key in _paramMap.Keys)
            {
                sqlCommand.Parameters.AddWithValue(_paramMap[key], key);
            }

            return sqlCommand;
        }

        /// <summary>
        /// Builds and returns a parameterized string based on the string that was provided. 
        /// NOTE: All interpolated values WILL be parameterized, so ensure you are only
        /// passing in primatives or other non-complex types (right now)
        /// </summary>
        /// <param name="sql">A SQL string that may / may not contain interpolated values</param>
        /// <returns>A parameterized SQL string</returns>
        public string Build(FormattableString sql)
        {
            // Get all interpolated values and parameterize them
            var pArgs = new List<string>();
            foreach (var argument in sql.GetArguments())
            {
                switch (argument)
                {
                    case ValueTuple<IList, bool> args:
                        // If the second parameter is false, then it's been requested that this parameter simply
                        // be passed through as expected.
                        if (!args.Item2)
                        {
                            pArgs.Add(Sanitize(args.Item1));
                        }
                        else
                        {
                            pArgs.Add(Parameterize(args));
                        }

                        break;
                    case ValueTuple<string, bool> arg:
                        // If the second parameter is false, then it's been requested that this parameter simply
                        // be passed through as expected.
                        if (!arg.Item2)
                        {
                            pArgs.Add(arg.Item1);
                        }
                        else
                        {
                            pArgs.Add(Parameterize(arg.Item1));
                        }

                        break;
                    case ValueTuple<object, bool> arg:
                        // If the second parameter is false, then it's been requested that this parameter simply
                        // be passed through as expected.
                        if (!arg.Item2)
                        {
                            pArgs.Add($"{ arg.Item1 }");
                        }
                        else
                        {
                            pArgs.Add(Parameterize(arg.Item1));
                        }

                        break;
                    case IList args:
                        pArgs.Add(Parameterize(args));
                        break;
                    default:
                        pArgs.Add(Parameterize(argument));
                        break;
                }
            }

            // Automatically handle parameterizing interpolated strings
            var result = string.Format(sql.Format, pArgs.ToArray());
            _stringBuilder.Append(result);

            return result;
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


        private string Sanitize(IList parameters)
        {
            var paramNames = new List<string>();
            foreach (var parameter in parameters)
            {
                paramNames.Add($"{ parameter }");
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
