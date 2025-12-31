using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
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
        inputService.OnDebugSwitchPressed += OnDebugSwitched;
    }

    public void Deactivate()
    {
        inputService.OnDebugSwitchPressed -= OnDebugSwitched;
    }

    private void OnDebugSwitched()
    {
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
    }

    private void OnLayout(UImGui.UImGui uImGui)
    {
        model.OnLayout(uImGui);
    }
}
