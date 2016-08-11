using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    /// <summary>
    /// Builds a middleware pipeline after receiving the pipeline from a pipeline provider
    /// </summary>
    public class MiddlewarePipelineBuilderService
    {
        private readonly ConcurrentDictionary<Type, RequestDelegate> _pipelinesCache
            = new ConcurrentDictionary<Type, RequestDelegate>();

        public IApplicationBuilder ApplicationBuilder { get; set; }

        public RequestDelegate GetPipeline(Type type)
        {
            // Build the pipeline only once. This is similar to how middlewares are used where they are constructed
            // only once.

            RequestDelegate requestDelegate;
            if (!_pipelinesCache.TryGetValue(type, out requestDelegate))
            {
                var pipelineProvider = (IMiddlewarePipelineProvider)Activator.CreateInstance(type);

                var nestedAppBuilder = ApplicationBuilder.New();

                // Get the user provided pipeline
                pipelineProvider.GetPipeline(nestedAppBuilder);

                // Attach a middleware in the end so that it continues the execution of rest of the MVC filter pipeline
                nestedAppBuilder.Run((httpContext) =>
                {
                    var filterContext = httpContext.Items[typeof(ResourceFilterContext)] as ResourceFilterContext;
                    var resourceExecutionDelegate = filterContext.ResourceExecutionDelegate;

                    return resourceExecutionDelegate();
                });

                requestDelegate = nestedAppBuilder.Build();

                _pipelinesCache.TryAdd(type, requestDelegate);
            }

            return requestDelegate;
        }
    }
}
