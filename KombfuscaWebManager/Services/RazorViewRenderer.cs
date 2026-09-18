using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace KombfuscaWebManager.Services
{

    public class RazorViewRenderer
    {
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewRenderer(
            ICompositeViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderAsync<TModel>(
            string viewName,
            TModel model,
            ControllerContext controllerContext)
        {
            var viewResult = viewName.StartsWith("~/", StringComparison.Ordinal)
                || viewName.StartsWith("/", StringComparison.Ordinal)
                ? _viewEngine.GetView(
                    executingFilePath: null,
                    viewPath: viewName,
                    isMainPage: false)
                : _viewEngine.FindView(
                    controllerContext,
                    viewName,
                    isMainPage: false);

            if (!viewResult.Success)
            {
                viewResult = _viewEngine.FindView(
                    controllerContext,
                    viewName,
                    isMainPage: false);
            }

            if (!viewResult.Success)
                throw new InvalidOperationException(
                    $"View '{viewName}' não encontrada."
                );

            await using var writer = new StringWriter();

            var viewData = new ViewDataDictionary<TModel>(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary()
            )
            {
                Model = model
            };

            var tempData = new TempDataDictionary(
                controllerContext.HttpContext,
                _tempDataProvider
            );

            var viewContext = new ViewContext(
                controllerContext,
                viewResult.View,
                viewData,
                tempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);

            return writer.ToString();
        }
    }
}
