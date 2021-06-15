using AngleSharp.Html.Parser;
using NerdStore.WebApp.MVC.Test.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace NerdStore.WebApp.MVC.Test
{
    [Collection(nameof(IntergrationWebTestFixtureCollection))]
    public class PedidoWebTests
    {
        private readonly IntegrationTestFixture<StartupWebTests> _testFixture;

        public PedidoWebTests(IntegrationTestFixture<StartupWebTests> testFixture)
        {
            _testFixture = testFixture;
        }

        [Fact(DisplayName = "Adicionar item em novo pedido")]
        [Trait("Categoria", "Integração Web - Pedido")]
        public async Task AdicionarItem_NovoPeido_DeveAtualizarValorTotal()
        {
            var produtoId = new Guid("9e0695d2-fc3a-48bc-bfa3-590a36af17ea");
            const int quantidade = 1;

            var initialResponse = await _testFixture.Client.GetAsync($"/produto-detalhe/{produtoId}");
            initialResponse.EnsureSuccessStatusCode();

            await _testFixture.LogarWeb();

            var formData = new Dictionary<string, string>
            {
                {"Id", produtoId.ToString() },
                { "quantidade", quantidade.ToString()}
            };

            var messgeRequest = new HttpRequestMessage(HttpMethod.Post, "/meu-carrinho")
            {
                Content = new FormUrlEncodedContent(formData)
            };

            //act
            var postResponse = await _testFixture.Client.SendAsync(messgeRequest);

            var html = new HtmlParser()
                .ParseDocumentAsync(await postResponse.Content.ReadAsStringAsync())
                .Result
                .All;

            var formQuantidade = html?.FirstOrDefault(s => s.Id == "quantidade")?.GetAttribute("value");

            

        }

    }
}
