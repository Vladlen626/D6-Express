using UImGui;

public abstract class DebugWindowModel
{
    protected bool isOpen;

    public abstract string Id { get; }

    public void Open()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
        UImGuiUtility.Layout += OnLayout;
    }

    public void Close()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        UImGuiUtility.Layout -= OnLayout;
    }

    protected abstract void OnLayout(UImGui.UImGui uImGui);
}
