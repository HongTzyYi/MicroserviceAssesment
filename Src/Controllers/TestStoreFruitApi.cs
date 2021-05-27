// <copyright file="TestStoreFruitApi.cs" company="Grass Valley">
// Copyright (c) Grass Valley. All rights reserved.
// </copyright>

#pragma warning disable SA1005,SA1201,SA1507,SA1512,SA1611,SA1614,SA1629,SA1633,SA1636,SA1641,CS1573 //suppress static code analysis

namespace Test.Store.Fruit.Controllers
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using FruitDataOperationController;
    using GV.Platform.Logging;
    using GV.Platform.MultiTenancy;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// Controller for TestStoreFruit API
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = false)]
    [Description("Controller for TestStoreFruit API.")]
    public class TestStoreFruitApi : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestStoreFruitApi"/> class.
        /// </summary>
        /// <param name="tenantContext">Tenant.</param>
        public TestStoreFruitApi(ILogging logger, ITenantContext tenantContext)
        {
            TenantContext = tenantContext;
        }

        private ILogging Logger { get; } 

        private ITenantContext TenantContext { get; }

        private FruitDataOperation fruitDataOps = new FruitDataOperation();

        /// <summary>
        /// Get Fruit stored in file
        /// </summary>
        /// <param name="id">Id of the fruit to find, <b>Required</b></param>
        /// <response code="200">Success</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="404">Not found.</response>
        /// <response code="500">Bad gateway.</response>
        /// <returns>Task.</returns>
        [HttpGet]
        [Route("/fruit/{id}")]
        [SwaggerOperation("getFruit")]
        [Produces("application/json")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "Return requested channel information.")]
        [SwaggerResponse(statusCode: 404, type: typeof(string), description: "Requested id not found.")]
        [Authorize(Policy = "platform.readonly")]
        [Authorize(Policy = "platform")]
        public virtual async Task<IActionResult> getFruitById([FromRoute(Name = "id")][Required]string id)
        {
            var response = string.Empty;
            await Task.Run(() =>  
            {
                response = fruitDataOps.ReadFruitData(id);

            });
            if (response.Contains("Exception"))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }

            return response == "success" ? StatusCode(StatusCodes.Status200OK, "Success") : StatusCode(StatusCodes.Status400BadRequest, response);
        }

        /// <summary>
        /// Get all Fruit stored in file
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="404">Not found.</response>
        /// <response code="500">Bad gateway.</response>
        /// <returns>Task.</returns>
        [HttpGet]
        [Route("/fruit")]
        [SwaggerOperation("getFruit")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "Return requested channel information.")]
        [SwaggerResponse(statusCode: 404, type: typeof(string), description: "Requested id not found.")]
        [Authorize(Policy = "platform.readonly")]
        [Authorize(Policy = "platform")]
        public virtual async Task<IActionResult> getFruit()
        {
            var response = string.Empty;
            await Task.Run(() =>
            {
                response = fruitDataOps.ReadFruitData();
            });
            if (response.Contains("Exception"))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }

            return response == "success" ? StatusCode(StatusCodes.Status200OK, "Success") : StatusCode(StatusCodes.Status400BadRequest, response);
        }

        /// <summary>
        /// Add Fruit to store in file
        /// </summary>
        /// <param name="name">Name of the new fruit to be added</param>
        /// <param name="color">Color for the new fruit to be added</param>
        /// <param name="availability">Number of the new fruit to keep in storage</param>
        /// <response code="200">Success</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="404">Not found.</response>
        /// <response code="500">Bad gateway.</response>
        /// <returns>Task.</returns>
        [HttpPost]
        [Route("/fruit")]
        [Produces("application/json")]
        [SwaggerOperation("addFruit")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "Added new Fruit data successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(string), description: "Add Fail.")]
        [Authorize(Policy = "platform.readonly")]
        [Authorize(Policy = "platform")]
        public virtual async Task<IActionResult> AddFruit(string name, string color, int availability)
        {
            var response = string.Empty;
            await Task.Run(() =>
            {
                response = fruitDataOps.addFruitData(name, color, availability);
            });

            return response == "success" ? StatusCode(StatusCodes.Status200OK, "Success") : StatusCode(StatusCodes.Status400BadRequest, response);
        }

        /// <summary>
        /// Delete Fruit to store in file
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="404">Not found.</response>
        /// <response code="500">Bad gateway.</response>
        /// <returns>Task.</returns>
        [HttpDelete]
        [Route("/fruit/{id}")]
        [SwaggerOperation("deleteFruit")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "Deleted fruit with supplied ID")]
        [SwaggerResponse(statusCode: 400, type: typeof(string), description: "Fail to Delete")]
        [Authorize(Policy = "platform.readonly")]
        [Authorize(Policy = "platform")]
        public virtual async Task<IActionResult> deleteFruitByID([FromRoute(Name = "id")][Required] string id)
        {
            var response = string.Empty;
            await Task.Run(() =>
            {
                response = fruitDataOps.RemoveFruitData(id);

            });
            return response == "success" ? StatusCode(StatusCodes.Status200OK, "Success") : StatusCode(StatusCodes.Status400BadRequest, response);
        }

        /// <summary>
        /// Update Fruit data in store
        /// </summary>
        /// <param name="id">ID of the fruit data to be updated, <b>Required</b></param>
        /// <param name="name">New name of the fruit of that ID</param>
        /// <param name="color">New color for the fruit of that ID</param>
        /// <param name="availability">Latest number of fruit available in storage</param>
        /// <response code="200">Success</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="404">Not found.</response>
        /// <response code="500">Bad gateway.</response>
        /// <returns>Task.</returns>
        [HttpPatch]
        [Route("/fruit/{id}")]
        [SwaggerOperation("updateFruit")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "Updated fruit data with supplied ID")]
        [SwaggerResponse(statusCode: 404, type: typeof(string), description: "Requested id not found.")]
        [Authorize(Policy = "platform.readonly")]
        [Authorize(Policy = "platform")]
        public virtual async Task<IActionResult> patchFruitById([FromRoute(Name = "id")][Required] string id, string name, string color, string availability)
        {
            var response = string.Empty;
            await Task.Run(() =>
            {
                response = fruitDataOps.UpdateFruitData(id, name, color, availability);

            });
            return response == "success" ? StatusCode(StatusCodes.Status200OK, "Success") : StatusCode(StatusCodes.Status400BadRequest, response);
        }
    }
}