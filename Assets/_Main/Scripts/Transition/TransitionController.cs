using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Audio;

public class TransitionController : IBaseController, IActivatable
{
    private readonly Run run;
    private readonly PlayerModel playerModel;
    private readonly PlayerView playerView;
    private readonly SceneContext sceneContext;
    private readonly IAudioService audioService;
    private readonly NpcSpawner npcSpawner;
    private readonly Shop shop;
    private readonly TransitionService transitionService;

    public TransitionController(Run run, PlayerModel playerModel, PlayerView playerView, SceneContext sceneContext, IAudioService audioService, NpcSpawner npcSpawner, Shop shop, TransitionService transitionService)
    {
        this.run = run;
        this.playerModel = playerModel;
        this.playerView = playerView;
        this.sceneContext = sceneContext;
        this.audioService = audioService;
        this.npcSpawner = npcSpawner;
        this.shop = shop;
        this.transitionService = transitionService;
    }

    public void Activate()
    {
        run.ProgressChanged += OnRunProgressChanged;
    }

    public void Deactivate()
    {
        run.ProgressChanged -= OnRunProgressChanged;
    }

    private async UniTask ChangeLocation()
    {
        sceneContext.TrainBlock.SetActive(run.Location == Location.TRAIN);
        sceneContext.StationBlock.SetActive(run.Location == Location.STATION);

        if (run.Location == Location.STATION)
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

    private void OnRunProgressChanged(Run.ProgressType progressType)
    {
        switch (progressType)
        {
            case Run.ProgressType.STARTED:
                OnRunStarted();
                break;
            case Run.ProgressType.DAY_FINISHED:
                OnDayChanged();
                break;
            case Run.ProgressType.SESSION_FINISHED:
                OnSessionChanged();
                break;
            case Run.ProgressType.LEVEL_FINISHED:
                OnLevelFinishedChanged();
                break;
            case Run.ProgressType.LOCATION_CHANGED:
                OnLocationChanged();
                break;
            case Run.ProgressType.WIN:
                OnRunFinished(true);
                break;
            case Run.ProgressType.LOSE:
                OnRunFinished(false);
                break;
        }
    }

    private async void OnRunStarted()
    {
        var data = new Transition.Data()
        {
            tasks = new[]
            {
                Transition.TaskType.CHANGE_LOCATION,
                Transition.TaskType.SHOP_RESTOCK,
                Transition.TaskType.NPC_RESPAWN,
            }
        };

        await transitionService.Request(data, GetTasks(data));
    }


    private async void OnLocationChanged()
    {
        var data = new Transition.Data()
        {
            tasks = new[]
            {
                Transition.TaskType.CHANGE_LOCATION,
                Transition.TaskType.SHOP_RESTOCK,
                Transition.TaskType.NPC_RESPAWN,
            }
        };

        await transitionService.Request(data, GetTasks(data));
    }

    private async void OnLevelFinishedChanged()
    {
        var data = new Transition.Data()
        {
            tasks = new[]
            {
                Transition.TaskType.WAKE_UP,
                Transition.TaskType.CHANGE_LOCATION,
                Transition.TaskType.SHOP_RESTOCK,
                Transition.TaskType.NPC_RESPAWN,
            }
        };

        await transitionService.Request(data, GetTasks(data));
    }

    private async void OnSessionChanged()
    {
        var data = new Transition.Data()
        {
            tasks = new[]
            {
                Transition.TaskType.SHOP_RESTOCK,
                Transition.TaskType.NPC_RESPAWN,
            }
        };

        await transitionService.Request(data, GetTasks(data));
    }

    private async void OnDayChanged()
    {
        var data = new Transition.Data()
        {
            tasks = new[]
            {
                Transition.TaskType.WAKE_UP,
                Transition.TaskType.SHOP_RESTOCK,
                Transition.TaskType.NPC_RESPAWN,
            }
        };

        await transitionService.Request(data, GetTasks(data));
    }

    private async void OnRunFinished(bool result)
    {
        var data = new Transition.Data()
        {
            tasks = new[]
            {
                result ? Transition.TaskType.WIN : Transition.TaskType.LOSE,
            }
        };

        await transitionService.Request(data, GetTasks(data));
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

    private IEnumerable<Func<UniTask>> GetTasks(Transition.Data data)
    {
        yield return () => StartTransition();

        foreach (var item in data.tasks)
        {
            if (item == Transition.TaskType.CHANGE_LOCATION)
            {
                yield return () => ChangeLocation();
            }
            else if (item == Transition.TaskType.NPC_RESPAWN)
            {
                yield return () => npcSpawner.Respawn();
            }
            else if (item == Transition.TaskType.SHOP_RESTOCK)
            {
                yield return () => UniTask.Create(async () => shop.Restock());
            }
        }

        yield return () => FinishTransition();
    }
}