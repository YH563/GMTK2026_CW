using Godot;
using System;

namespace GameTemplate.UI;

public partial class MainMenu : Control
{
	[ExportGroup("References")]
	[Export] public Button StartButton { get; set; }
	[Export] public Button QuitButton { get; set; }
	[Export] public Label TitleLabel { get; set; }

	public override void _Ready()
	{
		// 自动获取节点引用
		StartButton ??= GetNodeOrNull<Button>("VBoxContainer/StartButton");
		QuitButton ??= GetNodeOrNull<Button>("VBoxContainer/QuitButton");
		TitleLabel ??= GetNodeOrNull<Label>("TitleLabel");

		// 绑定按钮事件
		if (StartButton != null)
			StartButton.Pressed += OnStartPressed;

		if (QuitButton != null)
			QuitButton.Pressed += OnQuitPressed;
	}

	// 开始游戏按钮点击
	private void OnStartPressed()
	{
		Global.Instance?.ResetGame();
		Global.Instance?.LoadScene("res://scenes/Level.tscn");
	}

	// 退出游戏按钮点击
	private void OnQuitPressed()
	{
		Global.Instance?.QuitGame();
	}
}
