using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Audio;

public class TransitionController : IBaseController
{
    private readonly RunModel runModel;
    private readonly PlayerModel playerModel;
    private readonly PlayerView playerView;
    private readonly SceneContext sceneContext;
    private readonly IAudioService audioService;
    private readonly NpcSpawner npcSpawner;
    private readonly Shop shop;

    public TransitionController(RunModel runModel, PlayerModel playerModel, PlayerView playerView, SceneContext sceneContext, IAudioService audioService, NpcSpawner npcSpawner, Shop shop)
    {
        this.runModel = runModel;
        this.playerModel = playerModel;
        this.playerView = playerView;
        this.sceneContext = sceneContext;
        this.audioService = audioService;
        runModel.LevelModel.TickChanged += OnTickChanged;
        this.npcSpawner = npcSpawner;
        this.shop = shop;
    }

    public void StartObserving()
    {
        runModel.StateChanged += OnStateTransition;
    }

    public async Task StartLocationTransition()
    {
        await Locator.Resolve<TransitionService>().Request(new Transition.Data()
        {
            type = Transition.Type.LOCATION
        },
        () => StartTransition(),
        () => ChangeLocation(),
        () => npcSpawner.Respawn(),
        () => shop.Restock(),
        () => FinishTransition());
    }

    private async Task ChangeLocation()
    {
        sceneContext.TrainBlock.SetActive(runModel.LevelState == LevelState.TRAIN);
        sceneContext.StationBlock.SetActive(runModel.LevelState == LevelState.STATION);

        if (runModel.LevelState == LevelState.STATION)
        {
            audioService.StopParallelSound(SoundNames.TrainSound);
            audioService.PlaySoundParallel(SoundNames.StationSound);
            playerView.transform.SetPositionAndRotation(sceneContext.PlayerStationSpawnPosition.position,
                sceneContext.PlayerStationSpawnPosition.rotation);
        }
        else
        {
            audioService.StopParallelSound(SoundNames.StationSound);
            audioService.PlaySoundParallel(SoundNames.TrainSound);
            playerView.transform.SetPositionAndRotation(sceneContext.PlayerTrainSpawnPosition.position,
                sceneContext.PlayerStationSpawnPosition.rotation);
        }
    }

    private async void OnStateTransition()
    {
        await StartLocationTransition();
    }

    private async void OnTickChanged()
    {
        await Locator.Resolve<TransitionService>().Request(new Transition.Data()
        {
            type = Transition.Type.TICK
        },
        () => shop.Restock(),
        () => npcSpawner.Respawn());
    }

    private async Task StartTransition()
    {
        playerModel.PlayerStateModel.TryAddState(CharacterState.LOCATION_TRANSITIONING);
        playerView.SetCharacterGhost(true);
    }

    private async Task FinishTransition()
    {
        playerView.SetCharacterGhost(false);
        playerModel.PlayerStateModel.TryRemoveState(CharacterState.LOCATION_TRANSITIONING);
    }
}