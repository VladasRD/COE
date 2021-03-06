using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Box.Common.Web;
using SmartGeoIot.ViewModels;
using System.Linq;

namespace SmartGeoIot.Api
{
    [Route("api/[controller]")]
    public class SGIGraphicController : Controller
    {
        protected readonly SmartGeoIot.Services.SmartGeoIotService _sgiService;
        public SGIGraphicController(SmartGeoIot.Services.SmartGeoIotService sgiService)
        {
            _sgiService = sgiService;
        }

        [HttpGet("{id}")]
        [PaginationHeaderFilter]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SGI-GRAPHIC.READ")]
        public IEnumerable<DashboardViewModels> Get(string id, [FromQuery] string de, [FromQuery] string ate, [FromQuery] int skip = 0, [FromQuery] int top = 0)
        {
            if(id == null)
                throw new Box.Common.BoxLogicException("É necessário informar o id do dispositivo.");

            var deviceRegistration = _sgiService.GetDeviceRegistrationFull(id);
            var totalCount = new OptionalOutTotalCount();
            var reports = _sgiService.GetReports(id, User, deviceRegistration.Package.Type, skip, top, de, ate, totalCount, null, false, true).ToArray();
            Request.SetListTotalCount(totalCount.Value);

            return reports;
        }

    }
}