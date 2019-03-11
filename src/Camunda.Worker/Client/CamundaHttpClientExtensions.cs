// Copyright (c) Alexey Malinin. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Camunda.Worker.Client
{
    internal static class CamundaHttpClientExtensions
    {
        private const string JsonContentType = "application/json";
        private static readonly JsonSerializerSettings SerializerSettings = MakeSerializerSettings();

        private static JsonSerializerSettings MakeSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        ProcessDictionaryKeys = false,
                        OverrideSpecifiedNames = true
                    }
                },
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
            };
        }

        internal static async Task<HttpResponseMessage> PostJsonAsync(this HttpClient client,
            string path,
            object requestBody,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var jsonRequestBody = JsonConvert.SerializeObject(requestBody, SerializerSettings);
            var requestContent = new StringContent(jsonRequestBody, Encoding.UTF8, JsonContentType);
            var response = await client.PostAsync(path, requestContent, cancellationToken);
            return response;
        }

        internal static async Task<T> ReadAsObjectAsync<T>(this HttpContent content)
        {
            var jsonResponse = await content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(jsonResponse, SerializerSettings);
            return result;
        }
    }
}