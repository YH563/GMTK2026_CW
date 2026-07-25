using Godot;
using System.Collections.Generic;
using GameTemplate.Player;

namespace GameTemplate.Tricks;

public partial class Bread : Node2D
{
    [Export]
    public int GrowLength { get; set; } = 4;  // 生长长度
    [Export]
    public int MaxDistance { get; set; } = 100;

    [Export]
    public Node2D PlayerObject { get; set; }
    [Export]
    public float AnimDuration { get; set; } = 5f;

    [Export]
    public Node2D Middle;
    [Export]
    public Node2D Head;

    [Export]
    public Vector2 GrowDir { get; set; } = new Vector2(1, 0);  // 生长方向

    [ExportGroup("交互")]
    [Export]
    public InteractButton ButtonNode { get; set; }  // 关联的按钮区域

    private List<Node2D> _middles = new List<Node2D>();

    // 动画状态
    private bool _isAnimating = false;
    private float _animTime = 0f;
    private Timer _timer;
    private float _waitTime;
    private float _animHalfDuration = 0f;

    private Player.Player _player;

    public override void _Ready()
    {
        if (GrowDir.X * GrowDir.Y != 0 && (Mathf.Abs(GrowDir.X) != 1 || Mathf.Abs(GrowDir.Y) != 1))
        {
            GD.PrintErr("方向设置错误");
        }
        else
        {
            Rotation = GrowDir.Angle();
        }
        if (PlayerObject == null)
        {
            GD.PrintErr("[Bread] PlayerObject 未赋值！");
            return;
        }
        _player = PlayerObject.GetNode<Player.Player>("Body");
        _player.OnInteracted += TogglePan;

        _middles.Add(Middle);
        Head.Position = new Vector2 (18, 0);

        // 生长动画
        _timer = new Timer();
        AddChild(_timer);
        _animHalfDuration = AnimDuration / 2;
        _waitTime = _animHalfDuration / GrowLength;
        _timer.WaitTime = _waitTime;
        _timer.Timeout += Grow;
    }

    public override void _Process(double delta)
    {
        if (!_isAnimating) return;
        _animTime += (float)delta;
        if (_animTime > AnimDuration)
        {
            _isAnimating = false;
            _timer.Stop();  // 停止计时器
            if(_middles.Count == 2) ReduceMiddle();
        }
    }

    private void Grow()
    {
        if (_animTime <= _animHalfDuration) AddMiddle();
        if (_animTime > _animHalfDuration && _animTime <= AnimDuration) ReduceMiddle();
    }

    private void TogglePan()
    {
        // 玩家必须在按钮区域内才能触发
        if (ButtonNode != null && !ButtonNode.IsPlayerInRange) return;
        // 开启计时器
        if (_isAnimating) return;
        _isAnimating = true;
        _animTime = 0f;
        _timer.Start();
    }

    /// <summary>
    /// 伸长动画
    /// </summary>
    private void AddMiddle()
    {
        var middleClone = (Node2D)Middle.Duplicate();
        middleClone.Position = _middles[_middles.Count - 1].Position + new Vector2(18, 0);
        AddChild(middleClone);
        _middles.Add(middleClone);
        Head.Position = middleClone.Position + new Vector2(18, 0);
    }

    /// <summary>
    /// 缩回动画
    /// </summary>
    private void ReduceMiddle()
    {
        if (_middles.Count == 1) return;
        var lastMiddle = _middles[_middles.Count - 1];
        _middles.RemoveAt(_middles.Count - 1);
        lastMiddle.QueueFree();
        Head.Position -= new Vector2(18, 0);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (_player != null)
            _player.OnInteracted -= TogglePan;
    }
}
