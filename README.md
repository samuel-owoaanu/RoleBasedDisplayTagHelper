# RoleBasedDisplayTagHelper

RoleBasedDisplayTagHelper is a custom ASP.NET Core TagHelper for role and policy-based content rendering in Razor views. It helps you conditionally show or hide UI elements based on the authenticated user’s roles and authorization policies.

## Features

- **Conditional Display Based on Roles:**  
  Specify which roles can see certain content.
- **Excluding Certain Roles:**  
  Hide specific content from particular roles.
- **Authorization Policies:**  
  Leverage ASP.NET Core policies to refine access control.
- **Require Multiple Roles:**  
  Optionally require that a user has all specified roles.
- **Transparent Rendering:**  
  The wrapper tag is removed at runtime so only the inner content is rendered if conditions are met.

## Installation

1. **Via NuGet Package Manager Console:**
   ```powershell
   Install-Package RoleBasedDisplayTagHelper
   ```

2. **Via .NET CLI:**
   ```bash
   dotnet add package RoleBasedDisplayTagHelper
   ```

3. **Add to Your `_ViewImports.cshtml`:**
   ```cshtml
   @addTagHelper *, RoleBasedDisplayTagHelper
   ```
   
   **Identifying the Assembly Name:**  
   The assembly name defaults to your project’s name unless specified in the `.csproj`. If the assembly is `RoleBasedDisplayTagHelper`, use that in the `@addTagHelper` directive.

## Usage (Beginner-Friendly)

**Basic Role Check:**
```cshtml
<restricted-for include-roles="admin,manager">
    <p>This content is only visible to admins and managers.</p>
</restricted-for>
```

**Excluding Roles:**
```cshtml
<restricted-for exclude-roles="intern,guest">
    <p>This content is not visible to interns or guests.</p>
</restricted-for>
```

**Using Policies:**
```cshtml
<restricted-for policy="CanAccessAdminPanel">
    <p>Only users who meet the 'CanAccessAdminPanel' policy can see this.</p>
</restricted-for>
```

**Requiring All Roles:**
```cshtml
<restricted-for include-roles="admin,finance" require-all-roles="true">
    <p>You must be both 'admin' and 'finance' to see this content.</p>
</restricted-for>
```

**Combining Roles and Policies:**
```cshtml
<restricted-for policy="CanAccessReports" include-roles="manager" exclude-roles="intern">
    <p>Visible to managers who can access reports, but not to interns.</p>
</restricted-for>
```

**Nesting (Further Restriction):**
```cshtml
<restricted-for include-roles="staff">
    <h2>Staff Portal</h2>
    <restricted-for include-roles="manager">
        <h3>Management Only Section</h3>
    </restricted-for>
</restricted-for>
```

### Beginner Tips

- **Ensure Authentication is Setup:**  
  Make sure your application is using authentication and the current user principal has roles assigned. Without a valid user principal, the TagHelper will not show content.
  
- **Check Spelling:**  
  Policy or role name typos can cause the content to disappear. Double-check everything matches the definitions in your authorization configuration.

- **Use Browser Dev Tools:**  
  If you don’t see content, ensure it’s not a CSS issue. View the page source or use browser dev tools to confirm if the elements are actually missing or just visually hidden.

### Advanced User Tips

- **Define Your Own Authorization Policies:**  
  For more complex scenarios, implement custom authorization handlers and policies. For example:
  ```csharp
  services.AddAuthorization(options =>
  {
      options.AddPolicy("CanAccessAdminPanel", policy => policy.RequireRole("admin"));
  });
  ```

## Troubleshooting

- **Content Doesn’t Appear:**  
  - Check user roles and policies.
  - Confirm the user is authenticated.
  - Ensure `@addTagHelper *` `, RoleBasedDisplayTagHelper` is correct.

- **Tag Still Shows in the Rendered HTML:**  
  Make sure you have `output.TagName = null;` in the `ProcessAsync` method to remove the wrapper tag after checks succeed.

- **Assembly Name Issues:**  
  By default, the assembly name is the project name. You can find it in your `.csproj` under `<AssemblyName>` or by checking the `.dll` name in `bin/Release` or `bin/Debug`.