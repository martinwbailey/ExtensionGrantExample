// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Api.Controllers
{
    [Route("downstreamcall")]
    [Authorize]
    public class DownStreamCallController : ControllerBase
    {

        public async Task<IActionResult> Get()
        {
            //get the users token
            var userToken = await HttpContext.GetTokenAsync("access_token");

            var accessTokenResponse = await DelegateAsync(userToken);


            var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResponse.AccessToken);

            var content = await client.GetStringAsync("http://localhost:5003/values");

            var valueFromAPI2 = content.ToString();

            return Ok(valueFromAPI2);

        }

        private async Task<TokenResponse> DelegateAsync(string userToken)
        {
            var payload = new
            {
                token = userToken
            };
            var httpClient = new HttpClient();


            var disco = await httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = "http://localhost:5000",
                Policy = { ValidateIssuerName = false }
            });

            // create token client
            var client = new TokenClient(disco.TokenEndpoint, "api1.client", "secret");

            // send custom grant to token endpoint, return response
            return await client.RequestCustomGrantAsync("delegation", "api2", payload);
        }
    }
}