// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class MiddlewarePipelineResourceFilter : IAsyncResourceFilter, IOrderedFilter
    {
        private readonly RequestDelegate _requestDelegate;

        public MiddlewarePipelineResourceFilter(IApplicationBuilder nestedAppBuilder)
        {
            if (nestedAppBuilder == null)
            {
                throw new ArgumentNullException(nameof(nestedAppBuilder));
            }

            nestedAppBuilder.Run((httpContext) =>
            {
                var filterContext = httpContext.Items["ASPNETCORE_ResourceFilterContext"] as ResourceFilterContext;
                var resourceExecutionDelegate = filterContext.ResourceExecutionDelegate;

                return resourceExecutionDelegate();
            });

            _requestDelegate = nestedAppBuilder.Build();
        }

        public int Order
        {
            get
            {
                return int.MinValue + 100;
            }
        }

        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var httpContext = context.HttpContext;

            httpContext.Items.Add(
                "ASPNETCORE_ResourceFilterContext",
                new ResourceFilterContext()
                {
                    ResourceExecutionDelegate = next,
                    ResourceExecutingContext = context
                });

            return _requestDelegate(httpContext);
        }

        private class ResourceFilterContext
        {
            public ResourceExecutingContext ResourceExecutingContext { get; set; }

            public ResourceExecutionDelegate ResourceExecutionDelegate { get; set; }
        }
    }
}
