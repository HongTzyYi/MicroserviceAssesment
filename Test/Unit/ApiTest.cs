// <copyright file="ApiTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#pragma warning disable SA1005,SA1201,SA1507,SA1512,SA1611,SA1614,SA1629,SA1633,SA1636,SA1641,CS1573 //suppress static code analysis


namespace GV.SCS.Store.FridgeStore.Test.Unit
{
    using GV.Platform.Logging;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUlid;
    using Xunit;

    /// <summary>
    /// An example suite of unit tests.
    /// </summary>
    public class ApiTest
    {
        /// <summary>
        /// Expects a 200 response and the returned event to match that in store when requested event exists.
        /// </summary>
        [Fact]
        public void ShouldReturn200()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
