namespace PlatformCore.Services.UI
{
	public class UICursorView : UIBaseElement
	{
		protected override void OnShow()
		{
			base.OnShow();
			_group.interactable = false;
			_group.blocksRaycasts = false;
		}
	}
}