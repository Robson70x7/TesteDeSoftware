using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace NerdStore.WebApp.MVC.Test.Config
{
    [CollectionDefinition(nameof(IntergrationApiTestFixtureCollection))]
    public class IntergrationApiTestFixtureCollection : ICollectionFixture<IntegrationTestFixture<StartupApiTests>> { }


    [CollectionDefinition(nameof(IntergrationWebTestFixtureCollection))]
    public class IntergrationWebTestFixtureCollection : ICollectionFixture<IntegrationTestFixture<StartupWebTests>> { }

    public class IntegrationTestFixture<TStartup> : IDisposable where TStartup : class
    {
        public HttpClient Client;

        public readonly LojaAppFactory<TStartup> Factory;

        public string Email;
        public string Password;

        public IntegrationTestFixture()
        {
            var clientOptions = new WebApplicationFactoryClientOptions
            {
                HandleCookies = true,
                AllowAutoRedirect = true,
                BaseAddress = new Uri("http://localhost:5000"),
                MaxAutomaticRedirections = 5
            };

            //Create instance of application with the generic startup configuration;
            Factory = new LojaAppFactory<TStartup>();

            Client = Factory.CreateClient(clientOptions);
        }

        public void GerarUsuarioSenha()
        {
            var fk = new Faker("pt-BR");
            Email = fk.Internet.Email().ToLower();
            Password = fk.Internet.Password(8, false, prefix: "@1Ab_");
        }

        public async Task LogarWeb()
        {
            var initealReponse = await Client.GetAsync("/Identity/Account/Register");

            initealReponse.EnsureSuccessStatusCode();

            var token = ObterAntiForgetingToken(await initealReponse.Content.ReadAsStringAsync());

            var formData = new Dictionary<string, string>
            {
                {"Input.Email", "teste@123.com" },
                {"Input.Password", "S0u$@123" },
                {"Input.ConfirmPassword", Password },
                {"__RequestVerificationToken", token }
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/Identity/Account/Register")
            {
                Content = new FormUrlEncodedContent(formData)
            };

            await Client.SendAsync(requestMessage);
        }

        public string ObterAntiForgetingToken(string htmlBody)
        {
            var forgetKey = "__RequestVerificationToken";
            var pattern = $@"<input name=""__RequestVerificationToken"" type=""hidden"" value=""(.+)"" />";

            var resultMatch = Regex
                .Match(htmlBody, pattern);

            if (resultMatch.Success)
                return resultMatch.Groups[1].Captures[0].Value;
            
            throw new ArgumentException($"Anti forgety token {forgetKey} Não encontrado");
        }

        public void Dispose()
        {
            Client.Dispose();
            Factory.Dispose();
        }
    }
}
