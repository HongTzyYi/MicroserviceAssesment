// <copyright file="FruitDataOperation.cs" company="Grass Valley">
// Copyright (c) Grass Valley. All rights reserved.
// </copyright>

namespace GV.SCS.Store.FridgeStore.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Methods for CRUD operation of fruit data.
    /// </summary>
    public class FruitDataOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FruitDataOperation"/> class.
        /// </summary>
        public FruitDataOperation() { }

        private readonly string FruitDataStorePath = Path.Combine(Environment.CurrentDirectory, "FruitData.txt");

        /// <summary>
        /// Read available fruit data based on given id. Empty id will return the whole list of available fruit data instead.
        /// </summary>
        /// <param name="id">ID of the fruit data assigned.</param>
        /// <returns>JSON object of the fruit data of the given id. Return as array of json object if no id supplied.</returns>
        public string ReadFruitData(string id = "")
        {
            var result = string.Empty;
            try
            {
                if (File.Exists(FruitDataStorePath))
                {
                    var storedFruit = File.ReadAllLines(FruitDataStorePath);

                    if (!string.IsNullOrEmpty(id))
                    {
                        result = storedFruit.Where(s => s.Contains("ID=" + id)).FirstOrDefault();
                    }
                    else
                    {
                        var resultList = new List<Dictionary<string, string>>();
                        // Return whole list
                        foreach (var item in storedFruit)
                        {
                            var dict = new Dictionary<string, string>();
                            var currentEntry = item.Split('&');
                            foreach (var keyValuePair in currentEntry)
                            {
                                dict.Add(keyValuePair.Split('=').FirstOrDefault(), keyValuePair.Split('=').LastOrDefault());
                            }

                            resultList.Add(dict);
                        }

                        return JsonConvert.SerializeObject(resultList);
                    }
                }
            }
            catch (Exception ex)
            {
                return "Exception occurred: " + ex.ToString();
            }

            return string.IsNullOrEmpty(result) ? "No result with given ID:" + id : QueryStringToJson(result);
        }

        /// <summary>
        /// Add new fruit data.
        /// </summary>
        /// <param name="Name">Name for the new fruit data to be added.</param>
        /// <param name="Color">Color of the new fruit data.</param>
        /// <param name="Availability">Number of new fruit to be added.</param>
        /// <returns>"success" if add operation success, exception message if fail.</returns>
        public string addFruitData(string Name, string Color, int Availability)
        {
            var addingData = "ID=" + Guid.NewGuid().ToString("N") + "&" + "Name=" + Name + "&" + "Color=" + Color + "&" + "Availability=" + Availability;
            try
            {
                File.AppendAllText(FruitDataStorePath, addingData + Environment.NewLine);
            }
            catch (Exception ex)
            {
                return "Exception occurred: " + ex.ToString();
            }

            return "success";
        }

        /// <summary>
        /// Delete fruit data stored.
        /// </summary>
        /// <param name="id">ID of the fruit data to be deleted.</param>
        /// <returns>"success" if delete is completed, error message if fail.</returns>
        public string RemoveFruitData(string id)
        {
            try
            {
                if (File.Exists(FruitDataStorePath))
                {
                    var storedFruit = File.ReadAllLines(FruitDataStorePath);

                    if (!string.IsNullOrEmpty(id))
                    {
                        if (!storedFruit.Any(q => q.Contains("ID=" + id)))
                        {
                            return "No data exist for given ID";
                        }

                        var ToDelete = storedFruit.Where(s => !s.Contains("ID=" + id));
                        File.WriteAllLines(FruitDataStorePath, ToDelete);
                    }
                    else
                    {
                        return "ID not supplied";
                    }
                }
            }
            catch (Exception ex)
            {
                return "Exception occured:" + ex.ToString();
            }

            return "success";
        }

        /// <summary>
        /// Update available fruit data.
        /// </summary>
        /// <param name="id">ID of the fruit data to be updated.</param>
        /// <param name="name">New name for the ID of fruit data.</param>
        /// <param name="color">New color for the ID of the fruit data</param>
        /// <param name="availability">Number of fruit to be updated.</param>
        /// <returns>"success" if update is completed, else return error message if fail.</returns>
        public string UpdateFruitData(string id, string name, string color, string availability)
        {
            var newData = "ID=" + id + "&Name=" + name + "&Color=" + color + "&Availability=" + availability;
            try
            {
                if (File.Exists(FruitDataStorePath))
                {
                    var storedFruit = File.ReadAllLines(FruitDataStorePath);

                    if (!string.IsNullOrEmpty(id))
                    {
                        if (!storedFruit.Any(q => q.Contains("ID=" + id)))
                        {
                            return "No data exist for given ID";
                        }

                        var oldData = storedFruit.Where(s => s.Contains("ID=" + id)).FirstOrDefault();
                        var updatedData = storedFruit.Select(s => s.Replace(oldData, newData)).ToArray();
                        File.WriteAllLines(FruitDataStorePath, updatedData);
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
            catch (Exception ex)
            {
                return "Exception occurred: " + ex.ToString();
            }

            return "success";
        }

        /// <summary>
        /// Convert query string formatted string into JSON object.
        /// </summary>
        /// <param name="queryString">String in query string format.</param>
        /// <returns>JSON formatted string of the given input.</returns>
        public string QueryStringToJson(string queryString)
        {
            var dict = new Dictionary<string, string>();
            var keyValuePair = queryString.Split('&');
            foreach (var item in keyValuePair)
            {
                dict.Add(item.Split('=').FirstOrDefault(), item.Split('=').LastOrDefault());
            }
            return JsonConvert.SerializeObject(dict);
        }
    }
}
