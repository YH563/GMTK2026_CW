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

    [ExportGroup("倒计时显示")]
    [Export]
    public Label Label { get; set; }

    private List<Node2D> _middles = new List<Node2D>();

    // 动画状态
    private bool _isAnimating = false;
    private float _animTime = 0f;
    private int _growStep = 0;  // 计时器触发计数，用于区分生长/缩回阶段
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

        // 倒计时标签
        if (Label == null)
        {
            Label = new Label();
            Label.Name = "CountdownLabel";
            Label.Position = new Vector2(-40, -20);
            Label.HorizontalAlignment = HorizontalAlignment.Center;
            AddChild(Label);
        }
        // 反向旋转，让标签始终正向显示
        Label.Rotation = -Rotation;
        Label.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (!_isAnimating) return;
        _animTime += (float)delta;

        // 更新倒计时
        float remaining = AnimDuration - _animTime;
        if (remaining < 0f) remaining = 0f;
        Label.Text = Mathf.CeilToInt(remaining).ToString();

        // 安全网：超出总时长时强制结束动画（不停止计时器，由 Grow() 自行管理）
        if (_animTime > AnimDuration)
        {
            _isAnimating = false;
        }
    }

    private void Grow()
    {
        if (_growStep < GrowLength)
            AddMiddle();
        else
            ReduceMiddle();

        _growStep++;

        if (_growStep >= GrowLength * 2)
        {
            _isAnimating = false;
            _timer.Stop();
            Label.Visible = false;
        }
    }

    private void TogglePan()
    {
        // 玩家必须在按钮区域内才能触发
        if (ButtonNode != null && !ButtonNode.IsPlayerInRange) return;
        // 开启计时器
        if (_isAnimating) return;
        _isAnimating = true;
        _animTime = 0f;
        _growStep = 0;
        Label.Visible = true;
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
