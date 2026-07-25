using Godot;
using System;
using GameTemplate.Player;

namespace GameTemplate.Tricks;

public partial class Candle : Area2D
{
	[Export]
    public Node2D PlayerObject { get; set; }

	[Export]
	public Light2D Light { get; set; }

	[Export]
	public float LightDuration { get; set; } = 5f;  // 灯光持续时间（秒）

	private bool _playerInRange = false;
	private Player.Player _player;

	public override void _Ready()
	{
		if (PlayerObject == null)
		{
			GD.PrintErr("[Candle] PlayerObject 未赋值！");
			return;
		}

		_player = PlayerObject.GetNode<Player.Player>("Body");
		_player.OnInteracted += OnInteracted;

		// 初始关闭灯光
		if (Light != null)
			Light.Enabled = false;

		// 检测玩家进入/离开触发区域
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player.Player)
			_playerInRange = true;
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is Player.Player)
			_playerInRange = false;
	}

	private void OnInteracted()
	{
		if (!_playerInRange) return;
		if (Light == null) return;

		// 点亮灯光
		Light.Enabled = true;

		// 用 SceneTreeTimer 实现 LightDuration 秒后自动关闭
		var timer = GetTree().CreateTimer(LightDuration);
		timer.Timeout += () => Light.Enabled = false;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (_player != null)
			_player.OnInteracted -= OnInteracted;
	}
}
