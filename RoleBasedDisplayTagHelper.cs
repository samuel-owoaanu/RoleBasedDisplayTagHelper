using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RoleBasedDisplayTagHelper
{
    [HtmlTargetElement("restricted-for", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class RoleBasedDisplayTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public RoleBasedDisplayTagHelper(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        /// <summary>
        /// Comma-separated list of roles that are allowed to see the content.
        /// </summary>
        [HtmlAttributeName("include-roles")]
        public string IncludeRoles { get; set; }

        /// <summary>
        /// Comma-separated list of roles that are not allowed to see the content.
        /// </summary>
        [HtmlAttributeName("exclude-roles")]
        public string ExcludeRoles { get; set; }

        /// <summary>
        /// The name of an authorization policy to check.
        /// </summary>
        [HtmlAttributeName("policy")]
        public string Policy { get; set; }

        /// <summary>
        /// If set to "true", requires the user to have all specified roles (from IncludeRoles) instead of just one.
        /// </summary>
        [HtmlAttributeName("require-all-roles")]
        public bool RequireAllRoles { get; set; }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // If no user or not authenticated, hide content
            if (user == null || !user.Identity.IsAuthenticated)
            {
                output.SuppressOutput();
                return;
            }

            // Check policy if provided
            if (!string.IsNullOrWhiteSpace(Policy))
            {
                var authResult = await _authorizationService.AuthorizeAsync(user, Policy);
                if (!authResult.Succeeded)
                {
                    output.SuppressOutput();
                    return;
                }
            }

            var includeRolesList = ParseRoles(IncludeRoles);
            var excludeRolesList = ParseRoles(ExcludeRoles);

            // Check excluded roles
            if (excludeRolesList.Any(r => user.IsInRole(r)))
            {
                output.SuppressOutput();
                return;
            }

            // Check included roles
            if (includeRolesList.Any())
            {
                bool meetsRoleRequirement = RequireAllRoles
                    ? includeRolesList.All(user.IsInRole)
                    : includeRolesList.Any(user.IsInRole);

                if (!meetsRoleRequirement)
                {
                    output.SuppressOutput();
                    return;
                }
            }

            // If we got here, user passes all checks
            // Get the inner content and remove the tag wrapper
            var childContent = await output.GetChildContentAsync();
            output.Content.SetHtmlContent(childContent);
            // Console.WriteLine(output.Content.GetContent());
            output.TagName = null; // Remove the role-based tag from the final HTML
        }


        private static string[] ParseRoles(string roles)
        {
            if (string.IsNullOrWhiteSpace(roles))
            {
                return Array.Empty<string>();
            }

            return roles
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToArray();
        }
    }
}
