using Godot;
using System;

namespace GameTemplate.UI;

public partial class HUD : CanvasLayer
{
	[ExportGroup("References")]
	[Export] public Label ScoreLabel { get; set; }
	[Export] public Label LivesLabel { get; set; }
	[Export] public Label TimerLabel { get; set; }
	[Export] public Label GameOverLabel { get; set; }
	[Export] public Control PauseOverlay { get; set; }

	private bool _initialized;

	public override void _Ready()
	{
		// 自动查找 UI 节点
		ScoreLabel ??= GetNodeOrNull<Label>("ScoreLabel");
		LivesLabel ??= GetNodeOrNull<Label>("LivesLabel");
		TimerLabel ??= GetNodeOrNull<Label>("TimerLabel");
		GameOverLabel ??= GetNodeOrNull<Label>("GameOverLabel");
		PauseOverlay ??= GetNodeOrNull<Control>("PauseOverlay");

		// 初始隐藏 GameOver 和暂停遮罩
		if (GameOverLabel != null)
			GameOverLabel.Visible = false;

		if (PauseOverlay != null)
			PauseOverlay.Visible = false;

		// 连接全局事件
		if (Global.Instance != null)
		{
			Global.Instance.ScoreChanged += HandleScoreChanged;
			Global.Instance.LivesChanged += HandleLivesChanged;
			Global.Instance.GameOver += HandleGameOver;
			Global.Instance.GamePaused += HandleGamePaused;
			Global.Instance.GameResumed += HandleGameResumed;
		}

		_initialized = true;
		RefreshAll();
	}

	public override void _Process(double delta)
	{
		if (Global.Instance != null && TimerLabel != null)
		{
			float time = Global.Instance.GameTime;
			int minutes = Mathf.FloorToInt(time / 60);
			int seconds = Mathf.FloorToInt(time % 60);
			TimerLabel.Text = $"{minutes:00}:{seconds:00}";
		}
	}

	// 场景卸载时取消事件订阅，防止内存泄漏
	public override void _ExitTree()
	{
		if (Global.Instance != null)
		{
			Global.Instance.ScoreChanged -= HandleScoreChanged;
			Global.Instance.LivesChanged -= HandleLivesChanged;
			Global.Instance.GameOver -= HandleGameOver;
			Global.Instance.GamePaused -= HandleGamePaused;
			Global.Instance.GameResumed -= HandleGameResumed;
		}
	}

	// 刷新所有 UI 显示
	private void RefreshAll()
	{
		if (!_initialized || Global.Instance == null) return;
		HandleScoreChanged(Global.Instance.Score);
		HandleLivesChanged(Global.Instance.Lives);
	}

	// 分数变化回调
	private void HandleScoreChanged(int score)
	{
		if (ScoreLabel != null)
			ScoreLabel.Text = $"分数: {score}";
	}

	// 生命变化回调
	private void HandleLivesChanged(int lives)
	{
		if (LivesLabel != null)
			LivesLabel.Text = $"生命: {lives}";
	}

	// 游戏结束回调
	private void HandleGameOver()
	{
		if (GameOverLabel != null)
			GameOverLabel.Visible = true;
	}

	// 暂停回调
	private void HandleGamePaused()
	{
		if (PauseOverlay != null)
			PauseOverlay.Visible = true;
	}

	// 恢复回调
	private void HandleGameResumed()
	{
		if (PauseOverlay != null)
			PauseOverlay.Visible = false;
	}

	/// <summary>重新开始按钮点击处理</summary>
	public void OnRestartButtonPressed()
	{
		Global.Instance?.ReloadCurrentScene();
	}

	/// <summary>返回主菜单按钮点击处理</summary>
	public void OnMenuButtonPressed()
	{
		Global.Instance?.LoadScene("res://scenes/Main.tscn");
	}
}
