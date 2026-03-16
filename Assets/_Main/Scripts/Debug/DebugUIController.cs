using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.UI;
using UImGui;

public class DebugMenuUIController : IBaseController, IActivatable
{
    private bool opened;
    private readonly IInputService inputService;
    private readonly ICursorService cursorService;
    private readonly DebugMenuUIModel model;

    public DebugMenuUIController(IInputService inputService, ICursorService cursorService, DebugMenuUIModel model)
    {
        this.inputService = inputService;
        this.cursorService = cursorService;
        this.model = model;
    }

    public void Activate()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (model == null)
        {
            return;
        }

        inputService.OnDebugSwitchPressed += OnDebugSwitched;
#endif
    }

    public void Deactivate()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        inputService.OnDebugSwitchPressed -= OnDebugSwitched;

        if (opened)
        {
            UImGuiUtility.Layout -= OnLayout;
            inputService.EnableCameraInputs();
            cursorService.LockCursor();
            opened = false;
        }
#endif
    }

    private void OnDebugSwitched()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (model == null)
        {
            return;
        }

        if (opened)
        {
            UImGuiUtility.Layout -= OnLayout;
            inputService.EnableCameraInputs();
            cursorService.LockCursor();
        }
        else
        {
            UImGuiUtility.Layout += OnLayout;
            inputService.DisableCameraInputs();
            cursorService.UnlockCursor();
        }

        opened = !opened;
#endif
    }

    private void OnLayout(UImGui.UImGui uImGui)
    {
        if (model == null)
        {
            return;
        }

        model.OnLayout(uImGui);
    }
}
