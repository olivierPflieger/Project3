using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataShare_API.Filters
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Vérifie si la méthode ou le contrôleur possède notre attribut
            var isFileUpload = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<DisableFormValueModelBindingAttribute>()
                .Any() ?? false;

            if (isFileUpload)
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = {
                                    // "file" sera le nom du champ dans form-data
                                    ["file"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    },
                                    ["password"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description = "(Optionnel) Un mot de passe pour protéger le téléchargement de ce fichier."
                                    },
                                    ["tags"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description = "(Optionnel) Liste de tags séparés par des virgules (ex: projet, confidentiel, image)."
                                    },
                                    ["expirationDays"] = new OpenApiSchema
                                    {
                                        Type = "integer",
                                        Description = "(Optionnel) Nombre de jours avant que le fichier n'expire et soit supprimé. (Par défaut: 7)"
                                    }
                                },
                                Required = new HashSet<string> { "file" }
                            }
                        }
                    }
                };
            }
        }
    }
}
