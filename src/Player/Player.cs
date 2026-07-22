using Godot;
using System;

namespace GameTemplate.Player;

public partial class Player : CharacterBody2D
{
	[ExportGroup("Movement")]
	[Export] public float Speed { get; set; } = 200.0f;
	[Export] public float Acceleration { get; set; } = 1200.0f;
	[Export] public float Friction { get; set; } = 800.0f;

	[ExportGroup("Jump (Platformer Mode)")]
	[Export] public bool EnableJump { get; set; } = false;
	[Export] public float JumpVelocity { get; set; } = -400.0f;
	[Export] public float Gravity { get; set; } = 980.0f;

	[ExportGroup("Dash")]
	[Export] public bool EnableDash { get; set; } = false;
	[Export] public float DashSpeed { get; set; } = 600.0f;
	[Export] public float DashDuration { get; set; } = 0.2f;
	[Export] public float DashCooldown { get; set; } = 1.0f;

	[ExportGroup("References")]
	[Export] public AnimatedSprite2D AnimatedSprite { get; set; }
	[Export] public Area2D InteractionArea { get; set; }

	// 状态
	private Vector2 _moveDirection;
	private bool _isDashing;
	private float _dashTimer;
	private float _dashCooldownTimer;

	public override void _Ready()
	{
		if (AnimatedSprite == null)
			AnimatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

		if (InteractionArea == null)
			InteractionArea = GetNodeOrNull<Area2D>("InteractionArea");
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (EnableJump)
			HandlePlatformerMovement(dt);
		else
			HandleTopDownMovement(dt);
	}

	private void HandleTopDownMovement(float dt)
	{
		// 获取输入方向
		_moveDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");

		// 带加速/摩擦的平滑移动
		if (_moveDirection != Vector2.Zero)
		{
			Velocity = Velocity.MoveToward(_moveDirection * Speed, Acceleration * dt);
			UpdateAnimation(_moveDirection);
		}
		else
		{
			Velocity = Velocity.MoveToward(Vector2.Zero, Friction * dt);
		}

		MoveAndSlide();
	}

	private void HandlePlatformerMovement(float dt)
	{
		// 应用重力
		if (!IsOnFloor())
			Velocity = Velocity with { Y = Velocity.Y + Gravity * dt };

		// 水平输入
		float horizontalInput = Input.GetAxis("move_left", "move_right");
		Vector2 targetVelocity = new Vector2(horizontalInput * Speed, Velocity.Y);
		Velocity = new Vector2(
			Mathf.MoveToward(Velocity.X, targetVelocity.X, Acceleration * dt),
			targetVelocity.Y
		);

		// 跳跃
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			Velocity = new Vector2(Velocity.X, JumpVelocity);
		}

		// 冲刺
		if (EnableDash && Input.IsActionJustPressed("dash") && !_isDashing && _dashCooldownTimer <= 0)
		{
			StartDash();
		}

		if (_isDashing)
			UpdateDash(dt);

		_dashCooldownTimer -= dt;

		MoveAndSlide();
		UpdateAnimation(new Vector2(horizontalInput, 0));
	}

	private void StartDash()
	{
		_isDashing = true;
		_dashTimer = DashDuration;
		_dashCooldownTimer = DashCooldown;
		Vector2 dashDir = _moveDirection != Vector2.Zero ? _moveDirection.Normalized() : Vector2.Right;
		Velocity = dashDir * DashSpeed;
	}

	private void UpdateDash(float dt)
	{
		_dashTimer -= dt;
		if (_dashTimer <= 0)
		{
			_isDashing = false;
			Velocity *= 0.5f;
		}
	}

	private void UpdateAnimation(Vector2 direction)
	{
		if (AnimatedSprite == null) return;

		// 根据方向翻转精灵
		if (direction.X != 0)
			AnimatedSprite.Scale = new Vector2(Mathf.Sign(direction.X), 1);

		// 播放动画（需要在编辑器中创建 walk 和 idle 动画）
		if (AnimatedSprite.SpriteFrames.HasAnimation("walk") && direction != Vector2.Zero)
			AnimatedSprite.Play("walk");
		else if (AnimatedSprite.SpriteFrames.HasAnimation("idle"))
			AnimatedSprite.Play("idle");
	}

	/// <summary>获取角色当前面朝的方向</summary>
	/// <returns>单位方向向量</returns>
	public Vector2 GetFacingDirection()
	{
		if (_moveDirection != Vector2.Zero)
			return _moveDirection.Normalized();
		return Vector2.Right * Mathf.Sign(AnimatedSprite?.Scale.X ?? 1);
	}
}
