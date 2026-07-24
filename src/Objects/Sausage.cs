using Godot;
using System.Collections.Generic;
using GameTemplate.Player;

namespace GameTemplate.Tricks;

[Tool]
public partial class Sausage : Node2D
{
	[Export]
	public int Length{get; set;} = 3;

	[Export]
	public int MaxDistance{get; set;} = 100;

	[Export]
	public Node2D PlayerObject{get;set;}
	[Export]
	public float AnimDuration { get; set; } = 5f;
	[Export]
	public Vector2 MoveDir { get; set; } = new Vector2(1,0);  // 移动方向

	// 你需要几个字段来跟踪动画状态
	private bool _isAnimating = false;
	private float _animTime = 0f;
	private float _animHalfDuration = 0f;
	private float _animSpeed = 0f;
	private bool turn = false;  // 转向

	private Player.Player _player;

	public override void _Ready()
	{
		if (MoveDir.X * MoveDir.Y != 0 && (float.Abs(MoveDir.X) != 1 || float.Abs(MoveDir.Y) != 1))
		{
			GD.PrintErr("方向设置错误");
		}
		_player = PlayerObject.GetNode<Player.Player>("Body");
		_player.OnInteracted += TogglePan;
		_animHalfDuration = AnimDuration / 2;
		_animSpeed = MaxDistance / _animHalfDuration;

		if(Length > 3)
		{
			var middle = GetNodeOrNull<CharacterBody2D>("Middle");
			var right = GetNodeOrNull<CharacterBody2D>("Right");
			if (middle != null && right != null)
			{
				for(int i = 2;i<Length-1;i++)
				{
					var clone = (CharacterBody2D)middle.Duplicate();
					clone.Position = new Vector2(i * 18, 0);
					AddChild(clone);
				}
				right.Position = new Vector2((Length - 1) * 18, 0);
			}
			else
			{
				GD.PrintErr("[Sausage] 未找到子对象 Middle 或 Right");
			}
		}
	}

	public override void _Process(double delta)
	{
		if (!_isAnimating) return;
		_animTime += (float)delta;
		if (_animTime >= AnimDuration)
		{
			_isAnimating = false;
            MoveDir = -MoveDir;
        }
		if (_animTime >= _animHalfDuration && turn)
		{
			turn = false;
			MoveDir = -MoveDir;
		}
		Position += _animSpeed * (float)delta * MoveDir;
	}

	private void TogglePan()
	{
		if (_isAnimating) return;
		_isAnimating = true;
		_animTime = 0f;
		turn = true;
	}

    public override void _ExitTree()
    {
        base._ExitTree();
        if (_player != null)
            _player.OnInteracted -= TogglePan;
	}
}
