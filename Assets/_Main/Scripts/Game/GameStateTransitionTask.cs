// todo: тут очень императивно становится. раньше это были абстрактные таски. сейчас это парные show/hide
public enum GameStateTransitionTask
{
	VISUAL_TRANSITION_START,
	START_RUN,
	CHANGE_LOCATION,
	NPC_RESPAWN,
	SHOP_RESTOCK,
	SHOW_WIN,
	HIDE_WIN,
	SHOW_LOSE,
	HIDE_LOSE,
	VISUAL_TRANSITION_FINISH,
	LOCK_CURSOR,
	UNLOCK_CURSOR,
	CHARACTER_TRANSITION_START,
	CHARACTER_TRANSITION_FINISH,
	// todo: подумать нужен ли этот кусок в таком виде. слишком императивно
	SHOW_STATS,
	AWAIT_STATS,
	HIDE_STATS,
	// 
	OTHER
}