using Avalonia.Web.Blazor;

namespace SmoothScroller.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        WebAppBuilder.Configure<SmoothScroller.App>()
            .SetupWithSingleViewLifetime();
    }
}