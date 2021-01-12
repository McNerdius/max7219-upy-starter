using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;

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

        [FunctionName( "PutSi7021" )]
        public async Task Put(
            [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "Si7021/put/{hardwareId}/" )] HttpRequest httpRequest,
            [Blob( "data/Si7021/{hardwareId}.json", FileAccess.Read )] Stream blobInput,
            [Blob( "data/Si7021/{hardwareId}.json", FileAccess.Write )] Stream blobOutput )
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
                    > 72 => false,
                    < 65 => true,
                    _ => null
                };

                if ( reading.SetState is bool s )
                {
                    await httpClient.GetAsync( s ? config["ifttt_on"] : config["ifttt_off"] );
                }
            }

            readings = readings.Take( 100 ).Prepend( reading );

            await JsonSerializer.SerializeAsync( blobOutput, readings );
        }

        [FunctionName( "GetSi7021" )]
        public async Task<IActionResult> Get(
        [HttpTrigger( AuthorizationLevel.Anonymous, "get", "post", Route = "Si7021/get/{hardwareId}/" )] HttpRequest http,
        [Blob( "data/Si7021/{hardwareId}.json", FileAccess.Read )] Stream blobInput )
        {
            var reading = blobInput switch
            {
                null => new Si7021_Reading(),
                _ => (await JsonSerializer.DeserializeAsync<IEnumerable<Si7021_Reading>>( blobInput )).First()
            };

            return new ContentResult
            {
                Content = JsonSerializer.Serialize( new { reading.RH, reading.F }, new JsonSerializerOptions { IgnoreNullValues = true } ),
                ContentType = "application/json"
            };
        }
    }
}
