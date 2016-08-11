using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Mvc
{
    public interface IMiddlewarePipelineProvider
    {
        void GetPipeline(IApplicationBuilder applicationBuilder);
    }
}
