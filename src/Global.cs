using Godot;
using System;

public partial class Global : Node
{
	/// <summary>全局单例实例</summary>
	public static Global Instance { get; private set; }

	// 游戏状态
	/// <summary>当前得分</summary>
	public int Score { get; set; }
	/// <summary>剩余生命数</summary>
	public int Lives { get; set; } = 3;
	/// <summary>游戏已进行时间（秒）</summary>
	public float GameTime { get; set; }
	/// <summary>是否暂停</summary>
	public bool IsPaused { get; set; }
	/// <summary>是否游戏结束</summary>
	public bool IsGameOver { get; set; }

	// 事件
	/// <summary>分数变化时触发</summary>
	public event Action<int> ScoreChanged;
	/// <summary>生命变化时触发</summary>
	public event Action<int> LivesChanged;
	/// <summary>游戏结束时触发</summary>
	public event Action GameOver;
	/// <summary>游戏暂停时触发</summary>
	public event Action GamePaused;
	/// <summary>游戏恢复时触发</summary>
	public event Action GameResumed;

	public override void _Ready()
	{
		Instance = this;
		ProcessMode = Node.ProcessModeEnum.Always;
	}

	public override void _Process(double delta)
	{
		if (!IsPaused && !IsGameOver)
		{
			GameTime += (float)delta;
		}

		// 按 Escape 切换暂停
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			TogglePause();
		}
	}


	/// <summary>增加分数</summary>
	/// <param name="points">分数</param>
	public void AddScore(int points)
	{
		Score += points;
		ScoreChanged?.Invoke(Score);
	}

	/// <summary>减少一条生命</summary>
	public void LoseLife()
	{
		Lives--;
		LivesChanged?.Invoke(Lives);
		if (Lives <= 0)
		{
			TriggerGameOver();
		}
	}

	/// <summary>触发游戏结束</summary>
	public void TriggerGameOver()
	{
		IsGameOver = true;
		GameOver?.Invoke();
	}

	/// <summary>切换暂停</summary>
	public void TogglePause()
	{
		IsPaused = !IsPaused;
		GetTree().Paused = IsPaused;
		if (IsPaused)
			GamePaused?.Invoke();
		else
			GameResumed?.Invoke();
	}

	/// <summary>重置游戏</summary>
	public void ResetGame()
	{
		Score = 0;
		Lives = 3;
		GameTime = 0;
		IsGameOver = false;
		IsPaused = false;
		GetTree().Paused = false;
	}

	/// <summary>切换场景</summary>
	/// <param name="scenePath">目标场景文件路径（如 res://scenes/Level.tscn）</param>
	public void LoadScene(string scenePath)
	{
		GetTree().ChangeSceneToFile(scenePath);
	}

	/// <summary>重新加载当前场景并重置游戏状态</summary>
	public void ReloadCurrentScene()
	{
		ResetGame();
		GetTree().ReloadCurrentScene();
	}

	/// <summary>退出游戏</summary>
	public void QuitGame()
	{
		GetTree().Quit();
	}
}
