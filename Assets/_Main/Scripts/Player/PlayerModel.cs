public class PlayerModel
{
	public InventoryModel InventoryModel {get; private set;}

	public PlayerStateModel PlayerStateModel {get; private set; }

	public PlayerModel()
	{
		InventoryModel = new InventoryModel();
		PlayerStateModel = new PlayerStateModel();
	}
}