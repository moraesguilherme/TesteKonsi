using Microsoft.AspNetCore.Mvc;
using System;
using TesteKonsi.Views;

namespace TesteKonsi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BeneficioController : ControllerBase
    {
        [HttpPost("buscar-beneficio")]
        public ActionResult<BeneficioResponseViewModel> BuscarBeneficio([FromBody] MatriculaRequestViewModel request)
        {
            try
            {
                var portalCrawler = new Crawler.PortalCrawler(request.Cpf, request.Login, request.Senha);
                var mensagem = portalCrawler.RealizarBuscaBeneficio();

                var response = new BeneficioResponseViewModel
                {
                    Mensagem = mensagem
                };

                return response;
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}