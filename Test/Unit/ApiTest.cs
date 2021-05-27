// <copyright file="ApiTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#pragma warning disable SA1005,SA1201,SA1507,SA1512,SA1611,SA1614,SA1629,SA1633,SA1636,SA1641,CS1573 //suppress static code analysis


namespace Test.Store.Fruit.Test.Unit
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUlid;
    using NUnit.Framework;
    using FruitDataOperationController;

    /// <summary>
    /// An example suite of unit tests.
    /// </summary>
    public class ApiTest
    {
        [TestFixture]
        public class FruitDataAPITest
        {
            private FruitDataOperation FruitDataOps = new FruitDataOperation();
            private Controllers.TestStoreFruitApi TestStoreFruitAPI = new Controllers.TestStoreFruitApi(null, null);

            /// <summary>
            /// Setup testing values before each test 
            /// </summary>
            [SetUp]
            public void SetupTest()
            {
                //Prepopulate some data into storage for delete/get testing
                var testingFilePath = Path.Combine(Environment.CurrentDirectory, "FruitData.txt");
                if (File.Exists(testingFilePath))
                {
                    File.Delete(testingFilePath); //Ensure test is done in clean storage each time
                }

                var testingData = "ID=2ad2d0d75de543b896e12ab6d28be97c&Name=TestFruit&Color=Rainbow&Availability=99";
                File.AppendAllText(testingFilePath, testingData + Environment.NewLine);
            }

            // Testing on Swagger API 
            [Test]
            [TestCase("Banana", "Yellow", 11)]
            [TestCase("Grapa", "Purple", 2)]
            [TestCase("Pecha", "Pink", 6)]
            public async Task AddFruit_APITest(string name, string color, int availabitiy)
            {
                var response = await TestStoreFruitAPI.AddFruit(name, color, availabitiy);
                var result = response as ObjectResult;
                Assert.That(result.StatusCode, Is.EqualTo(200));
            }

            [Test]
            public async Task GetFruit_APITest()
            {
                var response = await TestStoreFruitAPI.getFruit();
                var result = response as ObjectResult;
                Assert.That(JsonConvert.SerializeObject(result.Value), Is.Not.Null.And.Not.Empty);
            }

            [Test]
            [TestCase("2ad2d0d75de543b896e12ab6d28be97c")]
            public async Task GetFruitByID_APITest(string id)
            {
                var response = await TestStoreFruitAPI.getFruitById(id);
                var result = response as ObjectResult;
                Assert.That(JsonConvert.SerializeObject(result.Value), Is.Not.Null.And.Not.Empty);
            }

            [Test]
            [TestCase("2ad2d0d75de543b896e12ab6d28be97c")]
            public async Task DeleteFruitByID_APITest(string id)
            {
                var response = await TestStoreFruitAPI.deleteFruitByID(id);
                var result = response as ObjectResult;
                Assert.That(result.StatusCode, Is.EqualTo(200));
            }

            // Testing on data management methods
            [Test]
            [TestCase("Banana", "Yellow", 11)]
            [TestCase("Grapa", "Purple", 2)]
            [TestCase("Pecha", "Pink", 6)]
            public void AddFruitDataTest(string name, string color, int availabitiy)
            {
                var response = FruitDataOps.addFruitData(name, color, availabitiy);
                Assert.That(response, Is.EqualTo("success").IgnoreCase);
            }

            [Test]
            [TestCase("2ad2d0d75de543b896e12ab6d28be97c")]
            [TestCase("")]
            public void ReadFruitDataTest(string id)
            {
                var response = FruitDataOps.ReadFruitData(id);
                Assert.That(JsonConvert.SerializeObject(response), Is.Not.Null.And.Not.Empty);
            }

            [Test]
            [TestCase("2ad2d0d75de543b896e12ab6d28be97c")]
            public void RemoveFruitData(string id)
            {
                var response = FruitDataOps.RemoveFruitData(id);
                Assert.That(response, Is.EqualTo("success").IgnoreCase);
            }
        }
    }
}
