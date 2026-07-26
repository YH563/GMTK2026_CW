using Godot;
using System;
using GameTemplate.Player;
using System.Collections.Generic;

namespace GameTemplate.Tricks;

public partial class MeltWall : Node
{
	[Export]
	public StaticBody2D Brick { get; set; }

    [Export]
    public int Length { get; set; } = 3;

    [Export]
    public Node2D PlayerObject { get; set; }
    [Export]
    public float AnimDuration { get; set; } = 5f;

    [ExportGroup("交互")]
    [Export]
    public InteractButton ButtonNode { get; set; }  // 关联的按钮区域

    [ExportGroup("倒计时显示")]
    [Export]
    public Label Label { get; set; }

    private bool _isAnimating = false;
    private float _animTime = 0f;
    private Timer _timer;
    private float _waitTime;

    private Player.Player _player;
    private StaticBody2D _brick;
    private List<StaticBody2D> _bricks = new List<StaticBody2D>();

    public override void _Ready()
    {
        _player = PlayerObject.GetNode<Player.Player>("Body");
        _player.OnInteracted += TogglePan;

        _brick = (StaticBody2D)Brick.Duplicate();
        Brick.QueueFree();
        MakeWall();

        // 生长动画
        _timer = new Timer();
        AddChild(_timer);
        _waitTime = AnimDuration / Length;
        _timer.WaitTime = _waitTime;
        _timer.Timeout += Melt;

        // 倒计时标签
        if (Label == null)
        {
            Label = new Label();
            Label.Name = "CountdownLabel";
            Label.Position = new Vector2(-40, -20);
            Label.HorizontalAlignment = HorizontalAlignment.Center;
            AddChild(Label);
        }
        Label.Visible = false;
        Label.Position = new Vector2(-9, -18 * Length - 12);
    }

    public override void _Process(double delta)
    {
        if (!_isAnimating) return;
        _animTime += (float)delta;
        float remaining = AnimDuration - _animTime;
        if (remaining < 0f) remaining = 0f;
        Label.Text = Mathf.CeilToInt(remaining).ToString();
        if (_animTime > AnimDuration)
        {
            _isAnimating = false;
            _timer.Stop();  // 停止计时器
            Label.Visible = false;
            if (_bricks.Count > 0) Melt();
            MakeWall() ;
        }
    }

    private void MakeWall()
    {
        for (int i = 0; i < Length; i++)
        {
            var brickClone = (StaticBody2D)_brick.Duplicate();
            brickClone.Position = new Vector2(0, -18 * i);
            _bricks.Add(brickClone);
            AddChild(brickClone);
        }
    }

    private void Melt()
    {
        if(_bricks.Count == 0) return;
        var brick = _bricks[_bricks.Count - 1];
        _bricks.RemoveAt(_bricks.Count - 1);
        brick.QueueFree();
    }

    public void TogglePan()
    {
        // 玩家必须在按钮区域内才能触发
        if (ButtonNode != null && !ButtonNode.IsPlayerInRange) return;
        if (_isAnimating) return;
        _isAnimating = true;
        _animTime = 0f;
        _timer.Start();
        Label.Visible = true;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (_player != null)
            _player.OnInteracted -= TogglePan;
    }
}
