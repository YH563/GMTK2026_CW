using Godot;
using System;
using GameTemplate.Player;
using System.Collections.Generic;

namespace GameTemplate.Tricks;

public partial class Bridge : Node2D
{
    [Export]
    public int Length { get; set; } = 3;

    [Export]
    public Node2D PlayerObject { get; set; }
    [Export]
    public float AnimDuration { get; set; } = 5f;

    [ExportGroup("交互")]
    [Export]
    public InteractButton ButtonNode { get; set; }  // 关联的按钮区域

    [ExportGroup("桥的部件")]
    [Export]
    public StaticBody2D Bridge1 { get; set; }
    [Export]
    public StaticBody2D Bridge2 { get; set; }
    [Export]
    public StaticBody2D Bridge3 { get; set; }

    [ExportGroup("倒计时显示")]
    [Export]
    public Label Label { get; set; }

    private StaticBody2D _bridge1;
    private StaticBody2D _bridge2;
    private StaticBody2D _bridge3;
    private LinkedList<StaticBody2D> _bridges;
    private Player.Player _player;

    // 动画相关设置
    private bool _isAnimating = false;
    private float _animTime = 0f;
    private Timer _timer;
    private float _waitTime;

    public override void _Ready()
    {
        _bridges = new LinkedList<StaticBody2D>();

        // 删除原本桥的部件
        if(Bridge1 != null && Bridge2 != null && Bridge3 != null)
        {
            _bridge1 = (StaticBody2D)Bridge1.Duplicate();
            _bridge1.Position = Vector2.Zero;
            _bridge2 = (StaticBody2D)Bridge2.Duplicate();
            _bridge2.Position = Vector2.Zero;
            _bridge3 = (StaticBody2D)Bridge3.Duplicate();
            _bridge3.Position = Vector2.Zero;
        }
        Bridge1.QueueFree();
        Bridge2.QueueFree();
        Bridge3.QueueFree();
        _player = PlayerObject.GetNode<Player.Player>("Body");
        _player.OnInteracted += TogglePan;

        _timer = new Timer();
        AddChild(_timer);
        _waitTime = AnimDuration / (Length / 2 + 1);
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
            if (_bridges.Count > 0) Melt();
        }
    }

    private void TogglePan()
    {
        // 玩家必须在按钮区域内才能触发
        if (ButtonNode != null && !ButtonNode.IsPlayerInRange) return;
        // 开启计时器
        if (_isAnimating) return;
        MakeBridge();
        _isAnimating = true;
        _animTime = 0f;
        _timer.Start();
    }

    /// <summary>
    /// 按下按钮时触发
    /// </summary>
    private void MakeBridge()
    {
        if (Length <= 2)
        {
            var bridge1 = (StaticBody2D)_bridge1.Duplicate();
            var bridge3 = (StaticBody2D)_bridge3.Duplicate();
            bridge3.Position = new Vector2(18, 0);
            _bridges.AddLast(bridge1);
            _bridges.AddLast(bridge3);
            AddChild(bridge1);
            AddChild(bridge3);
        }
        if (Length >= 3)
        {
            var bridge1 = (StaticBody2D)_bridge1.Duplicate();
            var bridge3 = (StaticBody2D)_bridge3.Duplicate();
            _bridges.AddLast(bridge1);
            AddChild(bridge1);
            for(int i=0; i<Length-2; i++)
            {
                var bridgeClone = (StaticBody2D)_bridge2.Duplicate();
                bridgeClone.Position = new Vector2(18 * (i+1), 0);
                _bridges.AddLast(bridgeClone);
                AddChild(bridgeClone);
            }
            bridge3.Position = new Vector2(18 * (Length - 1), 0);
            _bridges.AddLast(bridge3);
            AddChild(bridge3);
        }
        Label.Position = new Vector2((Length - 2) * 9 , -12);
        Label.Visible = true;
    }

    /// <summary>
    /// 融化动画
    /// </summary>
    private void Melt()
    {
        if (_bridges.Count == 0) return;
        var end = _bridges.Last;
        var begin = _bridges.First;
        if (end != null &&  begin != null && end != begin)
        {
            _bridges.RemoveFirst();
            _bridges.RemoveLast();
            end.Value.QueueFree();
            begin.Value.QueueFree();
        }
        if (end == begin)
        {
            _bridges.RemoveFirst();
            end.Value.QueueFree();
        }
    }
}
