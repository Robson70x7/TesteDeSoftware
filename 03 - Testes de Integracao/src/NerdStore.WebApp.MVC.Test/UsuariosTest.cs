using NerdStore.WebApp.MVC.Test.Config;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NerdStore.WebApp.MVC.Test
{
    [Collection(nameof(IntergrationWebTestFixtureCollection))]
    public class UsuariosTest
    {
        private readonly IntegrationTestFixture<StartupWebTests> _testFixture;

        public UsuariosTest(IntegrationTestFixture<StartupWebTests> testFixture)
        {
            _testFixture = testFixture;
        }

        [Fact(DisplayName = "Realizar Cadastro com Sucesso")]
        [Trait("Categoria", "Integração Web - Usuario")]
        public async Task Trocar_Nome_Metodo()
        {
            // Arrange
            var initealReponse = await _testFixture.Client.GetAsync("/Identity/Account/Register");

            initealReponse.EnsureSuccessStatusCode();

            var token = _testFixture.ObterAntiForgetingToken(await initealReponse.Content.ReadAsStringAsync());

            _testFixture.GerarUsuarioSenha();

            var formData = new Dictionary<string, string>
            {
                {"Input.Email", _testFixture.Email },
                {"Input.Password", _testFixture.Password },
                {"Input.ConfirmPassword", _testFixture.Password },
                {"__RequestVerificationToken", token }
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/Identity/Account/Register")
            {
                Content = new FormUrlEncodedContent(formData)
            };

            // Act
            var response = await _testFixture.Client.SendAsync(requestMessage);

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            Assert.Contains($"Hello {_testFixture.Email}!",responseString);
        }
    }
}
