using _Main.Scripts.Core.Services;
using ImGuiNET;
using PlatformCore.Core;
using UImGui;
using UnityEngine;

public class DebugGameWindow : MonoBehaviour
{
    private bool opened;
    private IInputService input;
    private ICursorService cursor;

    private void Awake()
    {
        input = Locator.Resolve<IInputService>();
        cursor = Locator.Resolve<ICursorService>();
    }

    private void OnEnable()
    {
        input.OnDebugSwitchPressed += OnDebugSwitched;
    }

    private void OnDisable()
    {
        input.OnDebugSwitchPressed -= OnDebugSwitched;
    }

    private void OnDebugSwitched()
    {
        if (opened)
        {
            UImGuiUtility.Layout -= OnLayout;
            input.EnableCameraInputs();
            cursor.LockCursor();
        }
        else
        {
            UImGuiUtility.Layout += OnLayout;
            input.DisableCameraInputs();
            cursor.UnlockCursor();
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