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
            await _dbContext.CreateClusterAsync<TestUser>();
            // Inserting Data into user table
            _dbContext.InsertIfNotExists<TestUser>(new TestUser(1, "LyubovK", "Dubai"));
            _dbContext.InsertIfNotExists<TestUser>(new TestUser(2, "JiriK", "Toronto"));
            _dbContext.InsertIfNotExists<TestUser>(new TestUser(3, "IvanH", "Mumbai"));
            _dbContext.InsertIfNotExists<TestUser>(new TestUser(4, "LiliyaB", "Seattle"));
            _dbContext.InsertIfNotExists<TestUser>(new TestUser(5, "JindrichH", "Buenos Aires"));
            Console.WriteLine("Inserted data into user table");
        }

        [TestMethod]
        public async Task Select_Test()
        {
            var user = await _dbContext.SelectAsync<TestUser>();

            Assert.IsNotNull(user);
        }


        [TestMethod]
        public async Task Select_With_Filter()
        {
            var user = await _dbContext.SelectAsync<TestUser>(k=>k.user_id == 1);

            Assert.AreEqual(user.First().user_name, "LyubovK");
        }

        [TestMethod]
        public async Task Insert_NewRow_ShouldReflect()
        {
            var userModel = new TestUser(6,"karuna","chennai");
            await _dbContext.InsertIfNotExistsAsync<TestUser>(userModel);

            Assert.IsNotNull(true);
        }


        [TestMethod]
        public async Task Update_ExistingRow_ShouldUpdated()
        {
            var userModel = new TestUser(2, "sp", "Mangoliya");
           // await _dbContext.UpdateAsync<TestUser>(c=>  , )};

            var updatedUser = await _dbContext.FirstOrDefaultAsync<TestUser>(k => k.user_id == 2 );

            Assert.AreSame(userModel,updatedUser);
        }

        [TestMethod]
        public void AddOrUpdate_createAnewRowOrUpdate_ShouldUpdateOrInsert()
        {
            var userModel = new TestUser(5, "karan", "chennai");
            _dbContext.AddOrUpdate<TestUser>(userModel);

            Assert.IsTrue(true);
        }



        [TestMethod]
        public async Task CreateCluster()
        {
            try
            {
              
                await _dbContext.CreateClusterAsync<TestSurvey>();

                var cc = _dbContext.InsertIfNotExistsAsync(new TestSurvey()
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

                await _dbContext.CreateClusterAsync<TestStudent>();

                await _dbContext.InsertIfNotExistsAsync(new TestStudent()
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
            var users = await _dbContext.SelectAsync<TestStudent>(c=>c.Standard == "SSLC");

            var fistUser = users.ToList();

            Assert.AreEqual("SSLC", fistUser.First().Standard);
        }


        [TestMethod]
        public async Task CreateNestedCluster()
        {
            try
            {

                await _dbContext.CreateClusterAsync<TestNestedModel>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
