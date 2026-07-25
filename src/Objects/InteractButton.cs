using Godot;
using GameTemplate.Player;

namespace GameTemplate.Tricks;

/// <summary>
/// 交互按钮触发器（Area2D）
/// 外部机关持有此按钮的引用，在按交互键时查询 IsPlayerInRange 即可。
/// </summary>
public partial class InteractButton : Area2D
{
    /// <summary>玩家当前是否在按钮区域内</summary>
    public bool IsPlayerInRange { get; private set; }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player.Player)
            IsPlayerInRange = true;
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player.Player)
            IsPlayerInRange = false;
    }
}
