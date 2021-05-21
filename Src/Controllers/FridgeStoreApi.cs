// <copyright file="FridgeStoreApi.cs" company="Grass Valley">
// Copyright (c) Grass Valley. All rights reserved.
// </copyright>

#pragma warning disable SA1005,SA1201,SA1507,SA1512,SA1611,SA1614,SA1629,SA1633,SA1636,SA1641,CS1573 //suppress static code analysis

namespace GV.SCS.Store.FridgeStore.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using GV.Platform.Logging;
    using GV.Platform.MultiTenancy;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Annotations;
    using System.Linq;

    /// debug: http://localhost:5000/swagger/index.html
    /// debug: http://localhost:5000/api/v1/store/FridgeStore/swagger/index.html
    /// <summary>
    /// Controller for FridgeStore API
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = false)]
    [Description("Controller for FridgeStore API.")]
    public class FridgeStoreApi : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FridgeStoreApi"/> class.
        /// </summary>
        /// <param name="tenantContext">Tenant.</param>
        public FridgeStoreApi(ILogging logger, ITenantContext tenantContext)
        {
            TenantContext = tenantContext;
        }

        private ILogging Logger { get; } 

        private ITenantContext TenantContext { get; }

        private string FruitDataStorePath = "FruitData.txt";

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
                response = ReadFruitData(id);

            });
            if (response.Contains("Exception"))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }

            return StatusCode(StatusCodes.Status200OK, response);
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
                response = ReadFruitData();
            });
            if (response.Contains("Exception"))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }

            return StatusCode(StatusCodes.Status200OK, response);
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
        [Route("/add")]
        [Produces("application/json")]
        [SwaggerOperation("addFruit")]
        [SwaggerResponse(statusCode: 200, type: typeof(string), description: "Added new Fruit data successfully")]
        [SwaggerResponse(statusCode: 404, type: typeof(string), description: "Requested id not found.")]
        [Authorize(Policy = "platform.readonly")]
        [Authorize(Policy = "platform")]
        public virtual async Task<IActionResult> AddFruit(string name, string color, int availability)
        {
            await Task.Run(() =>
            {
                var ret = addFruitData(name, color, availability);

            });
            return StatusCode(StatusCodes.Status200OK, "Success");
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
        [SwaggerResponse(statusCode: 404, type: typeof(string), description: "Requested id not found.")]
        [Authorize(Policy = "platform.readonly")]
        [Authorize(Policy = "platform")]
        public virtual async Task<IActionResult> deleteFruitByID([FromRoute(Name = "id")][Required] string id)
        {
            var response = string.Empty;
            await Task.Run(() =>
            {
                response = RemoveFruitData(id);

            });
            return StatusCode(StatusCodes.Status200OK, response);
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
                response = UpdateFruitData(id, name, color, availability);

            });
            return StatusCode(StatusCodes.Status200OK, response);
        }

        private string ReadFruitData(string id = "")
        {
            var result = string.Empty;
            try
            {
                if (System.IO.File.Exists(FruitDataStorePath))
                {
                    var storedFruit = System.IO.File.ReadAllLines(FruitDataStorePath);

                    if (!string.IsNullOrEmpty(id))
                    {
                        result = storedFruit.Where(s => s.Contains("ID=" + id)).FirstOrDefault();
                    }
                    else
                    {
                        var resultList = new List<Dictionary<string, string>>();
                        //Return whole list
                        foreach(var item in storedFruit)
                        {
                            var dict = new Dictionary<string, string>();
                            var currentEntry = item.Split('&');
                            foreach(var keyValuePair in currentEntry)
                            {
                                dict.Add(keyValuePair.Split('=').FirstOrDefault(), keyValuePair.Split('=').LastOrDefault());
                            }

                            resultList.Add(dict);
                        }
                        
                        return JsonConvert.SerializeObject(resultList);
                    }
                }
            }
            catch(Exception ex)
            {
                return "Exception occurred: " + ex.ToString();
            }

            return string.IsNullOrEmpty(result) ? "No result with given ID:" + id : QueryStringToJson(result);
        }

        private string addFruitData(string Name, string Color, int Availability)
        {
            var addingData = "ID=" + Guid.NewGuid().ToString("N") + "&" + "Name=" + Name + "&" + "Color=" + Color + "&" + "Availability=" + Availability;
            try
            {
                System.IO.File.AppendAllText(FruitDataStorePath, addingData + Environment.NewLine);
            }
            catch(Exception ex)
            {
                return "Exception occurred: " + ex.ToString();
            }
            
            return "success";
        }

        private string RemoveFruitData(string id)
        {
            try
            {
                if (System.IO.File.Exists(FruitDataStorePath))
                {
                    var storedFruit = System.IO.File.ReadAllLines(FruitDataStorePath);

                    if (!string.IsNullOrEmpty(id))
                    {
                        if (!storedFruit.Any(q => q.Contains("ID=" + id)))
                        {
                            return "No data exist for given ID";
                        }

                        var ToDelete = storedFruit.Where(s => !s.Contains("ID=" + id));
                        System.IO.File.WriteAllLines(FruitDataStorePath, ToDelete);
                    }
                    else
                    {
                        return "ID not supplied";
                    }
                }
            }
            catch(Exception ex)
            {
                return "Exception occured:" + ex.ToString();
            }

            return "success";
        }

        private string UpdateFruitData(string id, string name, string color, string availability)
        {
            var newData = "ID=" + id + "&Name=" + name + "&Color=" + color + "&Availability=" + availability;
            try
            {
                if (System.IO.File.Exists(FruitDataStorePath))
                {
                    var storedFruit = System.IO.File.ReadAllLines(FruitDataStorePath);

                    if (!string.IsNullOrEmpty(id))
                    {
                        if (!storedFruit.Any(q => q.Contains("ID=" + id)))
                        {
                            return "No data exist for given ID";
                        }

                        var oldData = storedFruit.Where(s => s.Contains("ID=" + id)).FirstOrDefault();
                        var updatedData = storedFruit.Select(s => s.Replace(oldData, newData)).ToArray();
                        System.IO.File.WriteAllLines(FruitDataStorePath, updatedData);
                    }
                    else
                    {
                        return "ID not supplied";
                    }
                }
                else
                {
                    return "Fruit data file missing or fail to read";
                }
            }
            catch(Exception ex)
            {
                return "Exception occurred: " + ex.ToString();
            }

            return "success";
        }

        private string QueryStringToJson(string queryString)
        {
            var dict = new Dictionary<string, string>();
            var keyValuePair = queryString.Split('&');
            foreach(var item in keyValuePair)
            {
                dict.Add(item.Split('=').FirstOrDefault(), item.Split('=').LastOrDefault());
            }
            return JsonConvert.SerializeObject(dict);
        }
    }
}