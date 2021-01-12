﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Karmatach.MaxPlay
{
    public class Si7021
    {
        private readonly IConfiguration config;
        private readonly HttpClient httpClient;

        public Si7021( IConfiguration Configuration, HttpClient httpClient )
        {
            this.config = Configuration;
            this.httpClient = httpClient;
        }

        [FunctionName( "SetSi7021" )]
        public async Task Set(
            [HttpTrigger( AuthorizationLevel.Anonymous, "get", "post", Route = "Si7021/set/{hardwareId}/" )] HttpRequest httpRequest,
            [Blob( "data/Si7021/{hardwareId}.json", FileAccess.Read )] Stream blobInput,
            [Blob( "data/Si7021/{hardwareId}.json", FileAccess.Write )] Stream blobOutput,
            ILogger log )
        {
            var readings = blobInput switch
            {
                null => Array.Empty<Si7021_Reading>(),
                _ => await JsonSerializer.DeserializeAsync<IEnumerable<Si7021_Reading>>( blobInput )
            };

            var reading = JsonSerializer.Deserialize<Si7021_Reading>( await httpRequest.ReadAsStringAsync() );
            var time = TimeZoneInfo.ConvertTimeFromUtc( DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById( "Mountain Standard Time" ) );
            reading.Time = time.ToString();

            var trendCount = 4;
            var recent = readings.Take( trendCount );

            // let the trend do trendy things for a bit, mmk
            if ( recent.Where( r => r.SetState is not null ).Count() == 0 )
            {
                var trend = recent.Sum( r => r.F ) / trendCount;

                reading.SetState = trend switch
                {
                    > 74 => false,
                    < 68 => true,
                    _ => null
                };

                if ( reading.SetState is bool s )
                {
                    await httpClient.GetAsync( s ? config["ifttt_on"] : config["ifttt_off"] );
                }

            }

            readings = readings.Take( 100 ).Prepend( reading );

            await JsonSerializer.SerializeAsync( blobOutput, readings, new JsonSerializerOptions { IgnoreNullValues = true } );
        }

        [FunctionName( "GetSi7021" )]
        public async Task<IActionResult> Get(
        [HttpTrigger( AuthorizationLevel.Anonymous, "get", "post", Route = "Si7021/get/{hardwareId}/" )] HttpRequest http,
        [Blob( "data/Si7021/{hardwareId}.json", FileAccess.Read )] Stream blobInput )
        {
            if ( blobInput is null )
            {
                return new NotFoundResult();
            }
            else
            {
                var reading = (await JsonSerializer.DeserializeAsync<IEnumerable<Si7021_Reading>>( blobInput )).First();

                return new ContentResult
                {
                    Content = JsonSerializer.Serialize( new { reading.RH, reading.F, reading.Time, reading.Battery } ),
                    ContentType = "application/json"
                };
            }
        }

        [FunctionName( "AllSi7021" )]
        public async Task<IActionResult> All(
        [HttpTrigger( AuthorizationLevel.Anonymous, "get", "post", Route = "Si7021/all/{hardwareId}/" )] HttpRequest http,
        [Blob( "data/Si7021/{hardwareId}.json", FileAccess.Read )] TextReader blobInput )
        {
            if ( blobInput is null )
            {
                return new NotFoundResult();
            }
            else
            {
                return new ContentResult
                {
                    Content = await blobInput.ReadToEndAsync(),
                    ContentType = "application/json"
                };
            }
        }
    }
}
