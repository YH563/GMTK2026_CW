using Godot;
using System;

namespace GameTemplate.Camera;

public partial class GameCamera : Camera2D
{
	[ExportGroup("Follow")]
	[Export] public Node2D Target { get; set; }
	[Export] public bool SmoothFollow { get; set; } = true;
	[Export] public float FollowSpeed { get; set; } = 8.0f;
	[Export] public Vector2 FollowOffset { get; set; } = Vector2.Zero;

	[ExportGroup("Limits")]
	[Export] public bool UseLimits { get; set; } = false;
	[Export] public Vector2 LimitMin { get; set; }
	[Export] public Vector2 LimitMax { get; set; }

	[ExportGroup("Shake")]
	[Export] public bool EnableShake { get; set; } = true;
	[Export] public float ShakeDecay { get; set; } = 5.0f;

	// 屏幕震动状态
	private float _shakeIntensity;
	private Vector2 _shakeOffset;

	public override void _Ready()
	{
		if (Target == null)
			Target = GetParentOrNull<Node2D>();

		if (UseLimits)
		{
			LimitLeft = (int)LimitMin.X;
			LimitTop = (int)LimitMin.Y;
			LimitRight = (int)LimitMax.X;
			LimitBottom = (int)LimitMax.Y;
		}
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;

		// 跟随目标
		if (Target != null)
		{
			Vector2 targetPos = Target.GlobalPosition + FollowOffset;

			if (SmoothFollow)
				GlobalPosition = GlobalPosition.Lerp(targetPos, FollowSpeed * dt);
			else
				GlobalPosition = targetPos;
		}

		// 屏幕震动效果
		if (_shakeIntensity > 0)
		{
			_shakeOffset = new Vector2(
				(float)GD.RandRange((double)-_shakeIntensity, (double)_shakeIntensity),
				(float)GD.RandRange((double)-_shakeIntensity, (double)_shakeIntensity)
			);
			Offset = _shakeOffset;
			_shakeIntensity = Mathf.MoveToward(_shakeIntensity, 0, ShakeDecay * dt);
		}
		else
		{
			Offset = Vector2.Zero;
		}
	}

	/// <summary>触发屏幕震动</summary>
	/// <param name="intensity">震动强度，默认 10</param>
	public void Shake(float intensity = 10.0f)
	{
		if (EnableShake)
			_shakeIntensity = Mathf.Max(_shakeIntensity, intensity);
	}

	/// <summary>设置摄像机跟随的目标节点</summary>
	/// <param name="target">要跟随的 Node2D 节点</param>
	public void SetTarget(Node2D target)
	{
		Target = target;
	}
}
