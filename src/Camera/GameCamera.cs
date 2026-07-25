using Godot;
using GameTemplate.Player;

namespace GameTemplate.Camera;

public partial class GameCamera : Camera2D
{
	[Export] public Node2D Target { get; set; }
	[Export] public float FollowSpeed { get; set; } = 6f;
	[Export] public Vector2 FollowOffset { get; set; } = Vector2.Zero;

	[Export] public bool EnableShake { get; set; } = true;
	[Export] public float ShakeDecay { get; set; } = 5f;

	private float _shakeIntensity;

	private Player.Player _player;

	public override void _Ready()
	{
		MakeCurrent();

		// 禁用 Camera2D 内置平滑 & 拖拽边距，避免干扰
		PositionSmoothingEnabled = false;
		// DragMarginsEnabled = false;

		Target ??= GetParentOrNull<Node2D>();

		if (Target != null)
		{
			GlobalPosition = Target.GlobalPosition + FollowOffset;
			_player = Target.GetNode<Player.Player>("Body");
		}
		else
		{
			GD.PrintErr("[Camera] Target 未赋值！");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Target == null) return;

		Vector2 targetPos = _player.GlobalPosition + FollowOffset;
		GlobalPosition = GlobalPosition.Lerp(targetPos, FollowSpeed * (float)delta);
	}

	public override void _Process(double delta)
	{
		if (_shakeIntensity > 0)
		{
			Offset = new Vector2(
				(float)GD.RandRange(-_shakeIntensity, _shakeIntensity),
				(float)GD.RandRange(-_shakeIntensity, _shakeIntensity)
			);
			_shakeIntensity = Mathf.MoveToward(_shakeIntensity, 0, ShakeDecay * (float)delta);
		}
		else
		{
			Offset = Vector2.Zero;
		}
	}

	public void Shake(float intensity = 10f)
	{
		if (EnableShake)
			_shakeIntensity = Mathf.Max(_shakeIntensity, intensity);
	}
}
