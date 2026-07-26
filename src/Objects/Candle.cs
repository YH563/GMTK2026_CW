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

    [ExportGroup("倒计时显示")]
    [Export]
    public Label Label { get; set; }

    private bool _playerInRange = false;
	private Player.Player _player;
	private bool _isLit = false;
	private float _lightTimer = 0f;

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

		// 倒计时标签初始化
		if (Label == null)
		{
			Label = new Label();
			Label.Name = "CountdownLabel";
			Label.Position = new Vector2(-40, -30);
			Label.HorizontalAlignment = HorizontalAlignment.Center;
			AddChild(Label);
		}
		Label.Rotation = -Rotation;
		Label.Visible = false;

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
		_isLit = true;
		_lightTimer = 0f;
		Label.Visible = true;
	}

	public override void _Process(double delta)
	{
		if (!_isLit) return;

		_lightTimer += (float)delta;
		float remaining = LightDuration - _lightTimer;
		if (remaining < 0f) remaining = 0f;
		Label.Text = Mathf.CeilToInt(remaining).ToString();

		if (_lightTimer >= LightDuration)
		{
			_isLit = false;
			Light.Enabled = false;
			Label.Visible = false;
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (_player != null)
			_player.OnInteracted -= OnInteracted;
	}
}
