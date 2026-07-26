using Godot;
using GameTemplate.Player;

namespace GameTemplate.Tricks;

/// <summary>
/// 酒/喷射机关：原始朝向朝上。
/// 配置一个 Area2D 作为触发区域，玩家进入后持续受到喷射方向的力。
/// </summary>
public partial class Wine : Node2D
{
    [ExportGroup("喷射")]
    [Export]
    public Vector2 JetDir { get; set; } = new Vector2(0, -1);  // 喷射方向（默认朝上）
    [Export]
    public float JetForce { get; set; } = 600f;  // 喷射力度

    [ExportGroup("交互")]
    [Export]
    public InteractButton ButtonNode { get; set; }  // 关联的按钮区域

    [ExportGroup("触发区域")]
    [Export]
    public Area2D TriggerArea { get; set; }  // 玩家在此区域内才受力

    [ExportGroup("粒子")]
    [Export]
    public GpuParticles2D Particles { get; set; }  // 粒子系统（设为 One Shot）

    [ExportGroup("玩家")]
    [Export]
    public Node2D PlayerObject { get; set; }  // 拖入场景中的 Player 节点

    [ExportGroup("倒计时显示")]
    [Export]
    public Label Label { get; set; }

    private Player.Player _player;
    private bool _playerInArea = false;    // 玩家是否在受力区域内
    private bool _isSpraying = false;      // 粒子喷射中

    // 倒计时
    private float _sprayTimer = 0f;        // 已喷射时间
    private float _sprayDuration = 0f;     // 喷射总时长（来自 Particles.Lifetime）

    public override void _Ready()
    {
        // 根据喷射方向旋转自身（原始朝向朝上，需补偿 +90°）
        if (JetDir == Vector2.Zero)
            GD.PrintErr("[Wine] JetDir 不能为零向量！");
        else
            Rotation = JetDir.Angle() + Mathf.Pi / 2;

        if (PlayerObject == null)
        {
            GD.PrintErr("[Wine] PlayerObject 未赋值！");
            return;
        }
        _player = PlayerObject.GetNode<Player.Player>("Body");
        _player.OnInteracted += OnInteracted;

        if (TriggerArea == null)
        {
            GD.PrintErr("[Wine] TriggerArea 未赋值！");
            return;
        }
        TriggerArea.BodyEntered += OnBodyEntered;
        TriggerArea.BodyExited += OnBodyExited;

        // 监听粒子播放结束
        if (Particles != null)
            Particles.Finished += OnParticlesFinished;

        // 倒计时标签初始化
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
        if (!_isSpraying) return;

        float remaining = _sprayDuration - _sprayTimer;
        if (remaining < 0f) remaining = 0f;
        Label.Text = Mathf.CeilToInt(remaining).ToString();
    }

    public override void _PhysicsProcess(double delta)
    {
        // 倒计时（独立于玩家位置，触发后持续计时）
        if (_isSpraying)
        {
            _sprayTimer += (float)delta;
            if (_sprayTimer >= _sprayDuration)
            {
                _isSpraying = false;
                Label.Visible = false;
                return;
            }
        }

        // 粒子喷射中 + 玩家在受力区域内 → 施加力
        if (!_isSpraying || !_playerInArea || _player == null) return;

        _player.ApplyForce(JetDir.Normalized() * JetForce * (float)delta);
    }

    private void OnInteracted()
    {
        // 玩家必须在按钮区域内
        if (ButtonNode != null && !ButtonNode.IsPlayerInRange) return;

        // 触发 one shot 粒子
        if (Particles != null)
        {
            Particles.Emitting = true;
            _isSpraying = true;
            _sprayTimer = 0f;
            _sprayDuration = (float)Particles.Lifetime;
            Label.Visible = true;
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player.Player)
            _playerInArea = true;
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player.Player)
            _playerInArea = false;
    }

    private void OnParticlesFinished()
    {
        // 粒子播放结束，喷射结束（_PhysicsProcess 中的计时器已优先处理）
        _isSpraying = false;
        Label.Visible = false;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (_player != null)
            _player.OnInteracted -= OnInteracted;
        if (TriggerArea != null)
        {
            TriggerArea.BodyEntered -= OnBodyEntered;
            TriggerArea.BodyExited -= OnBodyExited;
        }
        if (Particles != null)
            Particles.Finished -= OnParticlesFinished;
    }
}
