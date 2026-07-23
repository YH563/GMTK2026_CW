using Godot;

/// <summary>
/// 伸缩机关触发器——按下按键触发 AnimationPlayer 动画。
/// 
/// 用法：
/// 1. 把此脚本挂到场景任意节点上
/// 2. 把 AnimatableBody2D 下的 AnimationPlayer 拖入「Anim Player」
/// 3. 运行时按 E 键即可触发伸缩
/// </summary>
public partial class TelescopicTrick : Node
{
	[Export] public AnimationPlayer AnimPlayer { get; set; }
	[Export] public string ActionName { get; set; } = "interact";
	[Export] public string AnimName { get; set; } = "Trigger";

	public override void _Ready()
	{
		if (AnimPlayer == null)
			GD.PrintErr("[TelescopicTrick] AnimPlayer 未设置！请将 AnimationPlayer 拖入 Inspector");

		if (!InputMap.HasAction(ActionName))
			GD.PrintErr($"[TelescopicTrick] 输入动作 '{ActionName}' 不存在！请在项目设置中添加");
	}

	public override void _Input(InputEvent @event)
	{
		if (AnimPlayer == null) return;

		if (Input.IsActionJustPressed(ActionName))
		{
			if (AnimPlayer.IsPlaying())
			{
				AnimPlayer.Stop();
				AnimPlayer.Play("RESET");
			}
			else
			{
				AnimPlayer.Play(AnimName);
			}
		}
	}
}
