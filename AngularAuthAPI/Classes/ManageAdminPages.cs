using Microsoft.AspNetCore.Mvc.Rendering;

namespace AngularAuthAPI.Classes
{
    public static class ManageAdminPages
    {
        public static string Index => "Index";
        public static string Roles = "Roles";
        public static string Users = "Users";



        public static bool IsPageActive(ViewContext context, string page)
        {
            string? activePage = context.ViewData["ActivePage"]?.ToString();
            return string.Equals(activePage, page,StringComparison.OrdinalIgnoreCase) ? true : false;
        }
    }
}
