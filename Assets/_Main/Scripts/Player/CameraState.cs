using System;

[Serializable]
public struct CameraState
{
	public CharacterState characterState;
	public RotationType rotationType;
	public float minPitch;
	public float maxPitch;
	public float minYaw;
	public float maxYaw;
}