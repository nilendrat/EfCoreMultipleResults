using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace EfCoreMultipleResults
{
    public static class SPQueryBuilderMultipleResults
    {
        public static MultipleResultSetWrapper QueryMultipleResults(this DbContext db, string sql)
        {
            return new MultipleResultSetWrapper(db, sql);
        }

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
            /// Initializes a new instance of the <see cref="MultipleResultSetWrapper"/> class.
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
            /// <typeparam name="T"></typeparam>
            /// <param name="parameters">The parameters.</param>
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
        }
    }
}
