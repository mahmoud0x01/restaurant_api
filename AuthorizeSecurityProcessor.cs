// AuthorizeSecurityProcessor.cs
using Microsoft.AspNetCore.Authorization;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Collections.Generic;
using System.Linq;

internal class AuthorizeSecurityProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        // Check if the operation requires authorization
        var requiresAuthorization = context.MethodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();

        if (requiresAuthorization)
        {
            var securityRequirement = new NSwag.OpenApiSecurityRequirement
            {
                { "JWT", new string[] { } } // No specific scopes required
            };

            // Ensure the Security property is initialized
            if (context.OperationDescription.Operation.Security == null)
            {
                context.OperationDescription.Operation.Security = new List<NSwag.OpenApiSecurityRequirement>();
            }

            // Add the security requirement to the operation
            context.OperationDescription.Operation.Security.Add(securityRequirement);
        }

        return true;
    }
}
