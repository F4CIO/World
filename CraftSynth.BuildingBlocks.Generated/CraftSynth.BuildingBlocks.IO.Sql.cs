using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace CraftSynth.BuildingBlocks.IO
{
    public class Sql
    {
        public static bool TestConnectionString(string connectionString)
        {
            bool succeeded = false;

            throw new NotImplementedException(); //TODO: port to DotNet6
            //SqlConnection sqlConnection = new SqlConnection();
            //sqlConnection.ConnectionString = connectionString;

            //try
            //{
            //    sqlConnection.Open();
            //    succeeded = true;

            //}
            //catch (Exception exception)
            //{
            //    succeeded = false;

            //}
            //finally
            //{
            //    try
            //    {
            //        if (sqlConnection.State != System.Data.ConnectionState.Closed)
            //        {
            //            sqlConnection.Close();
            //        }
            //    }
            //    catch (Exception exception1) { }
            //}
            //return succeeded;
        }

    }
}
