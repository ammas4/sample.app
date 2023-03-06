using System.Text;
using AppSample.CoreTools.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AppSample.Api.Models;

public class BasicAuthHeaderBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var authHeader = bindingContext.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return Task.CompletedTask;
        }

        var prefix = "Basic ";
        if (authHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == false)
        {
            return Task.CompletedTask;
        }

        var authValue = authHeader.Substring(prefix.Length).Trim();

        string decodedValue;
        try
        {
            decodedValue = Encoding.UTF8.GetString(Convert.FromBase64String(authValue));
        }
        catch
        {
            //ошибка формата в base64 или utf8
            return Task.CompletedTask;
        }

        var decodedParts = decodedValue.Split(':', 2);
        if (decodedParts.Length > 1)
        {
            var login = decodedParts[0];
            var password = decodedParts[1];
            if (string.IsNullOrEmpty(login) == false)
            {
                bindingContext.Result = ModelBindingResult.Success(new BasicAuthHeaderInfo()
                {
                    Login = login, Password = password,
                });
            }
        }

        return Task.CompletedTask;
    }
}