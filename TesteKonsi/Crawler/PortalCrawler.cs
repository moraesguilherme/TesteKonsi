using HtmlAgilityPack;
using System.Net;
using System.Text;

namespace TesteKonsi.Crawler
{
    public class PortalCrawler
    {
        private readonly string _baseUrl = "http://extratoclube.com.br";
        private readonly string _cpf;
        private readonly string _usuario;
        private readonly string _senha;

        public PortalCrawler(string cpf, string usuario, string senha)
        {
            _cpf = cpf;
            _usuario = usuario;
            _senha = senha;
        }

        public static class HttpHelper
        {
            public static string PostDataToUrl(string url, List<KeyValuePair<string, string>> postData)
            {
                var client = new WebClient();
                var data = new StringBuilder();

                foreach (var pair in postData)
                {
                    data.Append($"{pair.Key}={WebUtility.UrlEncode(pair.Value)}&");
                }

                data.Length--;

                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var responseBytes = client.UploadData(url, "POST", Encoding.UTF8.GetBytes(data.ToString()));
                return Encoding.UTF8.GetString(responseBytes);
            }
        }

        private string GetHtmlFromUrl(string url)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                return client.DownloadString(url);
            }
        }

        public async Task<string> RealizarBuscaBeneficio()
        {
            // 1. Fazer a requisição para a página de login do portal
            var loginPageUrl = "http://extratoclube.com.br/";
            var httpClient = new HttpClient();
            var loginPageResponse = await httpClient.GetAsync(loginPageUrl);
            var loginPageHtml = await loginPageResponse.Content.ReadAsStringAsync();
            //teste

            // 2. Esperar a carga do script JavaScript antes de procurar pelo formulário de login
            await Task.Delay(1000); // aguarda 1 segundo

            var document = new HtmlDocument();
            document.LoadHtml(loginPageHtml);

            // Fechar popup de novidades recentes, caso esteja aberto
            var popup = document.DocumentNode.SelectSingleNode("//form[contains(@class,'login-form')]");
            if (popup != null)
            {
                var closeButton = popup.SelectSingleNode(".//a[@class='close-recent-news']");
                if (closeButton != null)
                {
                    var closeUrl = closeButton.Attributes["href"]?.Value;
                    if (!string.IsNullOrEmpty(closeUrl))
                    {
                        var fullCloseUrl = $"{loginPageUrl}/{closeUrl}";
                        await httpClient.GetAsync(fullCloseUrl);
                    }
                }
            }

            var usernameInput = document.DocumentNode.SelectSingleNode("//input[@name='login']");
            var form = usernameInput?.Ancestors("form").FirstOrDefault();

            if (form == null)
            {
                throw new Exception("Não foi possível encontrar o formulário de login");
            }

            var csrfTokenNode = form.SelectSingleNode(".//input[@name='csrf_token']");
            string csrfToken = null;
            if (csrfTokenNode != null && csrfTokenNode.Attributes["value"] != null)
            {
                csrfToken = csrfTokenNode.Attributes["value"].Value;
            }

            var csrfField = new KeyValuePair<string, string>("csrf_token", csrfToken);

            var postData = new List<KeyValuePair<string, string>>
            {
                csrfField,
                new KeyValuePair<string, string>("login", _usuario),
                new KeyValuePair<string, string>("senha", _senha)
            };

            var loginActionUrl = "http://extratoclube.com.br/login/logon/";
            var loginResultResponse = await httpClient.PostAsync(loginActionUrl, new FormUrlEncodedContent(postData));
            var loginResultHtml = await loginResultResponse.Content.ReadAsStringAsync();

            // 3. Verificar se o login foi bem-sucedido
            if (!loginResultHtml.Contains("Deslogar"))
            {
                throw new Exception("Não foi possível fazer login no portal");
            }

            // 4. Acessar a página de busca de benefícios
            var buscarBeneficiosUrl = "http://extratoclube.com.br/beneficios/busca/";
            var buscarBeneficiosResponse = await httpClient.GetAsync(buscarBeneficiosUrl);
            var buscarBeneficiosHtml = await buscarBeneficiosResponse.Content.ReadAsStringAsync();

            document.LoadHtml(buscarBeneficiosHtml);

            // 5. Preencher o campo de CPF e realizar a busca
            var cpfField = document.DocumentNode.SelectSingleNode("//input[@name='cpf']");
            if (cpfField != null)
            {
                cpfField.Attributes["value"].Value = _cpf;
            }

            var buscarButton = document.DocumentNode.SelectSingleNode("//button[@id='btn-buscar']");
            var buscarUrl = buscarButton?.Attributes["data-url"]?.Value;

            if (!string.IsNullOrEmpty(buscarUrl))
            {
                var fullBuscarUrl = $"{loginPageUrl}/{buscarUrl}";
                await httpClient.GetAsync(fullBuscarUrl);
            }

            // 6. Esperar a página de resultados carregar completamente
            await Task.Delay(5000);

            // 7. Extrair a mensagem do resultado da busca e retornar como resposta
            var resultadoHtml = GetHtmlFromUrl(buscarBeneficiosUrl);

            document.LoadHtml(resultadoHtml);

            var resultado = document.DocumentNode.SelectSingleNode("//div[@class='result-search']");
            var mensagem = resultado?.InnerText.Trim();

            return mensagem;
        }

    }
}