using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
namespace Shoes_Management
{
    public class AdminAuthorizationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext Context) 
        {
            var isadmin = Context.HttpContext.Session.GetString("is_admin");
            if (string.IsNullOrEmpty(isadmin))
            {
                Context.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    {"area","" },
                    {"controller","Home" },
                    {"action","Trangchu" }
                });
            }
        }

        public void OnActionExecuted(ActionExecutedContext Context) { }
    }
}
