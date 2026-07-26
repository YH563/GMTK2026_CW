using Godot;
using System;

namespace GameTemplate.Player;

/// <summary>
/// 平台跳跃角色控制器
/// 控制：A/D 或 ←/→ 左右移动，Space 跳跃，E 交互
/// </summary>
public partial class Player : CharacterBody2D
{
	// ===== 事件委托 =====
	/// <summary>按下交互键时触发，其他机关订阅此事件</summary>
	public event Action OnInteracted;

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

	// ===== 外力系统 =====
	private Vector2 _externalForce;
	private const float ExternalForceDamping = 0.85f;

	private Vector2 _rebrithPos;

	public override void _Ready()
	{
		SetRebrithPoint();
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

		// 5. 应用外力（叠加到速度上，然后逐渐衰减）
		if (_externalForce != Vector2.Zero)
		{
			Velocity += _externalForce;
			_externalForce *= ExternalForceDamping;
			if (_externalForce.LengthSquared() < 1f)
				_externalForce = Vector2.Zero;
		}

		// 6. 交互
		if (Input.IsActionJustPressed("interact"))
			OnInteracted?.Invoke();

		// 7. 执行移动
		MoveAndSlide();

		// 8. 更新动画
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		if (AnimatedSprite == null) return;

		// 左右翻转
		if (_horizontalInput != 0)
			AnimatedSprite.Scale = new Vector2(Mathf.Sign(_horizontalInput), 1);

		// 跳跃动画优先
		if (!IsOnFloor() && AnimatedSprite.SpriteFrames.HasAnimation("Jump"))
		{
			AnimatedSprite.Play("Jump");
		}
		// 切换地面动画
		else if (_horizontalInput != 0 && AnimatedSprite.SpriteFrames.HasAnimation("Walk"))
			AnimatedSprite.Play("Walk");
		else if (AnimatedSprite.SpriteFrames.HasAnimation("Idle"))
			AnimatedSprite.Play("Idle");
	}

	void SetRebrithPoint()
	{
		_rebrithPos = Position;
	}

	public void Remake()
	{
		Position = _rebrithPos;
	}

	/// <summary>由外部施加一个力（如击退、风、弹射）</summary>
	/// <param name="force">力的向量，会累加到现有外力上</param>
	public void ApplyForce(Vector2 force)
	{
		_externalForce += force;
	}

	/// <summary>获取面朝方向（单位向量）</summary>
	public Vector2 GetFacingDirection()
	{
		if (_horizontalInput != 0)
			return new Vector2(Mathf.Sign(_horizontalInput), 0);
		return Vector2.Right * Mathf.Sign(AnimatedSprite?.Scale.X ?? 1);
	}
}
