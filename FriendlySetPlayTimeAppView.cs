using Playnite;

namespace FriendlySetPlayTime;

public class FriendlySetPlayTimeTestAppView : AppViewItem
{
    public FriendlySetPlayTimeTestAppView()
    {
        View = new FriendlySetPlayTimeAppView
        {
            DataContext = this
        };
    }

    public override async Task ActivateViewAsync(ActivateViewAsyncArgs args)
    {
        // This gets called when the view is activated.
    }

    public override async Task DeactivateViewAsync(DeactivateViewAsyncArgs args)
    {
        // This gets called when the view is de-activated.
    }
}