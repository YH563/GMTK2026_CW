using Godot;

namespace GameTemplate.Player;

/// <summary>
/// 平台跳跃角色控制器
/// 控制：A/D 或 ←/→ 左右移动，Space 跳跃
/// </summary>
public partial class Player : CharacterBody2D
{
	// ===== 可调参数 =====
	[ExportGroup("移动")]
	[Export] public float Speed { get; set; } = 300.0f;
	[Export] public float Acceleration { get; set; } = 1200.0f;

	[ExportGroup("跳跃")]
	[Export] public float JumpVelocity { get; set; } = -450.0f;
	[Export] public float Gravity { get; set; } = 1200.0f;

	[ExportGroup("外观")]
	[Export] public AnimatedSprite2D AnimatedSprite { get; set; }

	// ===== 内部状态 =====
	private float _horizontalInput;

	public override void _Ready()
	{
		AnimatedSprite ??= GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// 1. 读取输入
		_horizontalInput = Input.GetAxis("move_left", "move_right");

		// 2. 应用重力
		if (!IsOnFloor())
			Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);

		// 3. 水平移动（带加速平滑）
		float targetSpeed = _horizontalInput * Speed;
		Velocity = new Vector2(
			Mathf.MoveToward(Velocity.X, targetSpeed, Acceleration * dt),
			Velocity.Y
		);

		// 4. 跳跃
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			Velocity = new Vector2(Velocity.X, JumpVelocity);

		// 5. 执行移动
		MoveAndSlide();

		// 6. 更新动画
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		if (AnimatedSprite == null) return;

		// 左右翻转
		if (_horizontalInput != 0)
			AnimatedSprite.Scale = new Vector2(Mathf.Sign(_horizontalInput), 1);

		// 切换动画
		if (_horizontalInput != 0 && AnimatedSprite.SpriteFrames.HasAnimation("walk"))
			AnimatedSprite.Play("walk");
		else if (AnimatedSprite.SpriteFrames.HasAnimation("idle"))
			AnimatedSprite.Play("idle");
	}

	/// <summary>获取面朝方向（单位向量）</summary>
	public Vector2 GetFacingDirection()
	{
		if (_horizontalInput != 0)
			return new Vector2(Mathf.Sign(_horizontalInput), 0);
		return Vector2.Right * Mathf.Sign(AnimatedSprite?.Scale.X ?? 1);
	}
}
