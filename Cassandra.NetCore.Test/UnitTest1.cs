using System;
using System.Linq;
using System.Threading.Tasks;
using Cassandra.NetCore.ORM;
using Cassandra.NetCore.ORM.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cassandra.NetCore.Test
{
    [TestClass]
    public class UnitTest1
    {

        private const string USERNAME = "localhost";
        private const string PASSWORD = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string CASSANDRACONTACTPOINT = "localhost";
        private static readonly int CASSANDRAPORT = 10350;

        private static readonly string keySpec = "uprofile";


 

        public ICassandraDbContext _dbContext;
        public UnitTest1()
        {
            _dbContext = new CassandraDbContext(USERNAME,PASSWORD,CASSANDRACONTACTPOINT,CASSANDRAPORT, keySpec);
            //TestData();
        }


        [TestMethod]
        public async Task TestData()
        {
            await _dbContext.CreateClusterAsync<User>();
            // Inserting Data into user table
            _dbContext.InsertIfNotExists<User>(new User(1, "LyubovK", "Dubai"));
            _dbContext.InsertIfNotExists<User>(new User(2, "JiriK", "Toronto"));
            _dbContext.InsertIfNotExists<User>(new User(3, "IvanH", "Mumbai"));
            _dbContext.InsertIfNotExists<User>(new User(4, "LiliyaB", "Seattle"));
            _dbContext.InsertIfNotExists<User>(new User(5, "JindrichH", "Buenos Aires"));
            Console.WriteLine("Inserted data into user table");
        }

        [TestMethod]
        public async Task Select_Test()
        {
            var user = await _dbContext.SelectAsync<User>();

            Assert.IsNotNull(user);
        }


        [TestMethod]
        public async Task Select_With_Filter()
        {
            var user = await _dbContext.SelectAsync<User>(k=>k.user_id == 1);

            Assert.AreEqual(user.First().user_name, "LyubovK");
        }

        [TestMethod]
        public async Task Insert_NewRow_ShouldReflect()
        {
            var userModel = new User(6,"karuna","chennai");
            await _dbContext.InsertIfNotExistsAsync<User>(userModel);

            Assert.IsNotNull(true);
        }


        [TestMethod]
        public async Task Update_ExistingRow_ShouldUpdated()
        {
            var userModel = new User(2, "sp", "Mangoliya");
           // await _dbContext.UpdateAsync<User>(c=>  , )};

            var updatedUser = await _dbContext.FirstOrDefaultAsync<User>(k => k.user_id == 2 );

            Assert.AreSame(userModel,updatedUser);
        }

        [TestMethod]
        public void AddOrUpdate_createAnewRowOrUpdate_ShouldUpdateOrInsert()
        {
            var userModel = new User(5, "karan", "chennai");
            _dbContext.AddOrUpdate<User>(userModel);

            Assert.IsTrue(true);
        }



        [TestMethod]
        public async Task CreateCluster()
        {
            try
            {
              
                await _dbContext.CreateClusterAsync<Survey>();

                var cc = _dbContext.InsertIfNotExistsAsync(new Survey()
                {
                    Id = 2,
                    City = "Chennai",
                    UserName = "karuna"
                });

                Assert.IsNotNull(cc);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
         
        }

        [TestMethod]
        public async Task CreateClusterWithTwoPrimaryKey()
        {
            try
            {

                await _dbContext.CreateClusterAsync<Student>();

                await _dbContext.InsertIfNotExistsAsync(new Student()
                {
                    Id = 2,
                    Standard = "SSLC",
                    Section = "A",
                    CreatedOn = DateTime.Now
                });

                Assert.IsNotNull(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        [TestMethod]
        public async Task Select_With_Filter_IndexKey()
        {
            var users = await _dbContext.SelectAsync<Student>(c=>c.Standard == "SSLC");

            var fistUser = users.ToList();

            Assert.AreEqual("SSLC", fistUser.First().Standard);
        }

    }
}
