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
    private readonly TransitionService transitionService;

    public TransitionController(RunModel runModel, PlayerModel playerModel, PlayerView playerView, SceneContext sceneContext, IAudioService audioService, NpcSpawner npcSpawner, Shop shop, TransitionService transitionService)
    {
        this.runModel = runModel;
        this.playerModel = playerModel;
        this.playerView = playerView;
        this.sceneContext = sceneContext;
        this.audioService = audioService;
        this.npcSpawner = npcSpawner;
        this.shop = shop;
        this.transitionService = transitionService;
    }

    public void StartObserving()
    {
        runModel.StateChanged += OnStateTransition;
        runModel.LevelModel.TickChanged += OnTickChanged;
    }

    public async Task StartLocationTransition()
    {
        await transitionService.Request(new Transition.Data()
        {
            type = Transition.Type.LOCATION
        },
        () => StartTransition(),
        () => ChangeLocation(),
        () => npcSpawner.Respawn(),
        () => UniTask.Create(async () => shop.Restock()),
        () => FinishTransition());
    }

    private async UniTask ChangeLocation()
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
        await transitionService.Request(new Transition.Data()
        {
            type = Transition.Type.TICK
        },
        () => UniTask.Create(async () => shop.Restock()),
        () => npcSpawner.Respawn());
    }

    private async UniTask StartTransition()
    {
        playerModel.PlayerStateModel.TryAddState(CharacterState.LOCATION_TRANSITIONING);
        playerView.SetCharacterGhost(true);
    }

    private async UniTask FinishTransition()
    {
        playerView.SetCharacterGhost(false);
        playerModel.PlayerStateModel.TryRemoveState(CharacterState.LOCATION_TRANSITIONING);
    }
}