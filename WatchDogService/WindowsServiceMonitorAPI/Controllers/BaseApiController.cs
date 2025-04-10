using Microsoft.AspNetCore.Mvc;

namespace WatchDogServiceApi.Controllers
{
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected IActionResult ApiResponse<T>(string message, bool result, T data)
        {
            return Ok(new { message, result, data });
        }

        internal int? CustomerId
        {
            get
            {
                var key = "CustomerId";
                if (Request.Headers.ContainsKey(key))
                    return Convert.ToInt32(Request.Headers[key][0]);
                else
                    return (int?)null;
            }
        }
        internal int? UserId
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    var nameIdClaim = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                    if (nameIdClaim != null && int.TryParse(nameIdClaim.Value, out int nameId))
                        return nameId;
                }

                return null;
            }
        }
        protected JsonResult InternalError500(Exception ex)
        {
            var er = ex.Message != null ? ex.Message : "An unexpected error occurred, can't provide more details";
            var dt = ex.InnerException != null ? ex.InnerException.Message : String.Empty;
            return new JsonResult(null) { Value = new { error = er, detail = dt }, StatusCode = 500 };
        }

        protected void LogRequest(HttpContext Context)
        {
            //Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm") + " | url:" + Context.Request.Path + " | from:" + Context.Connection.RemoteIpAddress);
        }
    }
}
