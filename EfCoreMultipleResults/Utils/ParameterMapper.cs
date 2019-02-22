using EfCoreMultipleResults.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace EfCoreMultipleResults.Utils
{
    public class ParameterMapper: IParameterMapper
    {
        public StoredProcedure Map<TSource>(TSource source)
        {
            Type type = source.GetType();

            var sp = new StoredProcedure
            {
                Name = type.Name
            };

            var parameters = new List<SqlParameter>();

            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (GetIfDataList(prop))
                {
                    DataTable dt = GenerateDataTableParameter(prop, (IList)prop.GetValue(source, null));
                    if (dt != null)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = dt.TableName;
                        parameter.SqlDbType = SqlDbType.Structured;
                        parameter.Value = dt;

                        parameters.Add(parameter);
                    }

                }
                else
                {
                    parameters.Add(new SqlParameter(prop.Name, prop.GetValue(source, null)));
                }
            }

            sp.Parameters = parameters.ToArray();

            return sp;
        }
        public bool GetIfDataList(PropertyInfo propertyInfo)
        {
            bool rtnValue = false;

            if (propertyInfo != null)
            {
                object[] atts = propertyInfo.GetCustomAttributes(typeof(SqlParameterConfig), true);

                if (atts.Any())
                {
                    var customeAttr = (SqlParameterConfig)atts[0];
                    rtnValue = customeAttr.IsDataList;
                }
            }

            return rtnValue;
        }

        public DataTable GenerateDataTableParameter(PropertyInfo propertyInfo, IList values)
        {
            if (values != null && values.Count > 0)
            {
                DataTable dt = new DataTable(propertyInfo.Name);

                Type type = values[0].GetType();

                foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    dt.Columns.Add(prop.Name, prop.PropertyType);
                }

                foreach (var item in values)
                {
                    DataRow dr = dt.NewRow();

                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        var colName = dt.Columns[i].ColumnName;
                        var value = item.GetType().GetProperty(colName).GetValue(item);
                        dr[i] = value;
                    }

                    dt.Rows.Add(dr);
                }

                return dt;
            }

            return null;

        }
    }
}
