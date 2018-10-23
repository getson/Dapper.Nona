using System.Data.SqlClient;
using System.IO;
using Dapper.Nona;
using Dapper.Nona.FluentMapping;
using Dapper.Nona.IntegrationTests.Entities;
using Dapper.Nona.IntegrationTests.Map;
using Dapper.FluentMap;

namespace Dapper.Nona.IntegrationTests
{
   public class BaseTests
   {
       private static object _locker = new object(); 
        static BaseTests()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            FluentMapper.Initialize(cfg =>
            {
                cfg.AddMap(new ProductMap());
                cfg.AddMap(new LargeProductMap());
                cfg.ApplyToNona();

            });
        }

        protected void DeleteAll()
        {
            lock(_locker)
            {
                using (var con = new SqlConnection(GetConnectionString()))
                {
                    con.DeleteAll<Product>();
                }
            }

        }
        protected static string GetConnectionString()
        {
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var fileName = Path.Combine(currentDir.Parent.Parent.Parent.FullName, "App_Data", "Nona.mdf");
            var conStr = "Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=True;AttachDBFilename=" + fileName;
            return conStr;
        }
    }
}
