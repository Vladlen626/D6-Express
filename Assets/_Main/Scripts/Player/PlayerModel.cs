public class PlayerModel
{
	public InventoryModel InventoryModel {get; private set;}

	public PlayerStateModel PlayerStateModel {get; private set; }

	public PlayerModel()
	{
		InventoryModel = new InventoryModel();
		PlayerStateModel = new PlayerStateModel();
	}
	
	public void Init(LevelModel levelModel)
	{
		this.levelModel = levelModel;
		this.levelModel.LevelStateChanged += OnLevelStateChanged;
	}
	
	private void OnLevelStateChanged()
	{
		SetCharacterState(CharacterState.LOCATION_TRANSITIONING);
	}

	public void SetupCharacterStateModel(CharacterStateHandler characterStateHandler)
	{
		
	}
}
