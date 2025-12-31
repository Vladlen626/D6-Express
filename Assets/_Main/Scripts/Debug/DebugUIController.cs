using _Main.Scripts.Core.Services;
using ImGuiNET;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UImGui;
using UnityEngine;

public class DebugUIController : IBaseController, IActivatable
{
    private bool opened;
    private readonly IInputService inputService;
    private readonly ICursorService cursorService;

    public DebugUIController(IInputService inputService, ICursorService cursorService)
    {
        this.inputService = inputService;
        this.cursorService = cursorService;
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
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Game"))
            {
                if (ImGui.MenuItem("Increment Tick"))
                {
                    Debug.Log("puk");
                }

                if (ImGui.MenuItem("Increment Day"))
                {
                    Debug.Log("srenk");
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}