using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EfCoreMultipleResults
{
    /// <summary>
    /// 
    /// </summary>
    public static class SPQueryBuilderMultipleResults
    {
        /// <summary>
        /// Queries the multiple results.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public static MultipleResultSetWrapper QueryMultipleResults(this DbContext db, string sql)
        {
            return new MultipleResultSetWrapper(db, sql);
        }

        /// <summary>
        /// 
        /// </summary>
        public class MultipleResultSetWrapper
        {
            /// <summary>
            /// The database
            /// </summary>
            private readonly DbContext _db;
            /// <summary>
            /// The stored procedure
            /// </summary>
            private readonly string _storedProcedure;
            /// <summary>
            /// Initializes a new instance of the <see cref="MultipleResultSetWrapper" /> class.
            /// </summary>
            /// <param name="db">The database.</param>
            /// <param name="sql">The SQL.</param>
            public MultipleResultSetWrapper(DbContext db, string sql)
            {
                _db = db;
                _storedProcedure = sql;
            }
            /// <summary>
            /// Executes the specified parameters.
            /// </summary>
            /// <param name="parameters">The parameters.</param>
            /// <param name="returnTypes">The return types.</param>
            /// <returns></returns>
            public List<List<dynamic>> ExecuteMultipleResults(object parameters, List<Type> returnTypes)
            {
                List<List<dynamic>> results = new List<List<dynamic>>();

                var connection = _db.Database.GetDbConnection();
                var command = connection.CreateCommand();
                command.CommandText = _storedProcedure;
                command.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    var paramType = parameters.GetType();

                    foreach (var pi in paramType.GetProperties())
                    {
                        DbParameter param = command.CreateParameter();
                        param.ParameterName = pi.Name;
                        param.DbType = SqlHelper.GetDbType(pi.PropertyType);
                        param.Direction = ParameterDirection.Input;
                        param.Value = paramType.GetProperty(pi.Name).GetValue(parameters);

                        command.Parameters.Add(param);
                    }
                }

                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }

                int counter = 0;

                using (var reader = command.ExecuteReader())
                {
                    do
                    {
                        List<dynamic> tempResults = new List<dynamic>();

                        while (reader.Read())
                        {

                            var t = Activator.CreateInstance(returnTypes[counter]);

                            for (int inc = 0; inc < reader.FieldCount; inc++)
                            {
                                Type type = t.GetType();
                                string name = reader.GetName(inc);
                                PropertyInfo prop = type.GetProperty(name);
                                if (prop != null && name == prop.Name)
                                {

                                    var value = reader.GetValue(inc);
                                    if (value != DBNull.Value)
                                    {
                                        prop.SetValue(t, Convert.ChangeType(value, prop.PropertyType), null);
                                    }
                                }

                            }
                            tempResults.Add(t);

                        }
                        results.Add(tempResults);
                        counter++;
                    }
                    while (reader.NextResult());
                    reader.Close();
                }

                return results;
            }

            /// <summary>
            /// Executes the multiple.
            /// </summary>
            /// <param name="parameters">The parameters.</param>
            /// <param name="types">The types.</param>
            /// <returns></returns>
            public List<List<dynamic>> ExecuteMultiple(SqlParameter[] parameters, params Type[] types)
            {
                List<List<dynamic>> results = new List<List<dynamic>>();

                var connection = _db.Database.GetDbConnection();
                var command = connection.CreateCommand();
                command.CommandText = _storedProcedure;
                command.CommandType = CommandType.StoredProcedure;

                if (parameters != null && parameters.Any())
                {
                    command.Parameters.AddRange(parameters);
                }

                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }

                int counter = 0;
                using (var reader = command.ExecuteReader())
                {
                    do
                    {
                        var innerResults = new List<dynamic>();

                        if (counter > types.Length - 1) { break; }

                        while (reader.Read())
                        {
                            var item = Activator.CreateInstance(types[counter]);

                            for (int inc = 0; inc < reader.FieldCount; inc++)
                            {
                                Type type = item.GetType();
                                string name = reader.GetName(inc);
                                PropertyInfo property = type.GetProperty(name);

                                if (property != null && name == property.Name)
                                {
                                    var value = reader.GetValue(inc);
                                    if (value != null && value != DBNull.Value)
                                    {
                                        property.SetValue(item, Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType), null);
                                    }
                                }
                            }
                            innerResults.Add(item);
                        }
                        results.Add(innerResults);
                        counter++;
                    }
                    while (reader.NextResult());
                    reader.Close();
                }
                return results;
            }
        }
    }
}
