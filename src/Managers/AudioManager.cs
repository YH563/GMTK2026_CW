using Godot;
using System;
using System.Collections.Generic;

namespace GameTemplate.Managers;

public partial class AudioManager : Node
{
	/// <summary>全局单例实例</summary>
	public static AudioManager Instance { get; private set; }

	[ExportGroup("Settings")]
	/// <summary>主音量（0~1）</summary>
	[Export] public float MasterVolume { get; set; } = 1.0f;
	/// <summary>音效音量（0~1）</summary>
	[Export] public float SfxVolume { get; set; } = 1.0f;
	/// <summary>音乐音量（0~1）</summary>
	[Export] public float MusicVolume { get; set; } = 0.8f;

	// 音频播放器
	private AudioStreamPlayer _musicPlayer;
	private AudioStreamPlayer _sfxPlayer;

	public override void _Ready()
	{
		Instance = this;
		ProcessMode = ProcessModeEnum.Always;

		// 创建音乐播放器和主音效播放器
		_musicPlayer = new AudioStreamPlayer { Name = "MusicPlayer" };
		_sfxPlayer = new AudioStreamPlayer { Name = "SfxPlayer" };
		AddChild(_musicPlayer);
		AddChild(_sfxPlayer);

		// 创建额外的音效播放器池，支持重叠播放
		for (int i = 0; i < 4; i++)
		{
			var extra = new AudioStreamPlayer { Name = $"SfxPlayer_{i}" };
			AddChild(extra);
		}

		UpdateVolumes();
	}

	/// <summary>播放背景音乐，支持淡入效果</summary>
	/// <param name="music">音乐音频流</param>
	/// <param name="fadeTime">淡入时间（秒），0 为立即播放</param>
	public void PlayMusic(AudioStream music, float fadeTime = 0)
	{
		if (fadeTime > 0)
		{
			var tween = CreateTween();
			tween.TweenProperty(_musicPlayer, "volume_db", -80, fadeTime);
			tween.TweenCallback(Callable.From(() =>
			{
				_musicPlayer.Stream = music;
				_musicPlayer.Play();
				var fadeIn = CreateTween();
				fadeIn.TweenProperty(_musicPlayer, "volume_db", LinearToDb(MusicVolume * MasterVolume), fadeTime);
			}));
		}
		else
		{
			_musicPlayer.Stream = music;
			_musicPlayer.Play();
		}
	}

	/// <summary>停止背景音乐，支持淡出效果</summary>
	/// <param name="fadeTime">淡出时间（秒），0 为立即停止</param>
	public void StopMusic(float fadeTime = 0)
	{
		if (fadeTime > 0)
		{
			var tween = CreateTween();
			tween.TweenProperty(_musicPlayer, "volume_db", -80, fadeTime);
			tween.TweenCallback(Callable.From(() => _musicPlayer.Stop()));
		}
		else
		{
			_musicPlayer.Stop();
		}
	}

	/// <summary>播放音效（自动使用空闲的音效播放器，支持重叠播放）</summary>
	/// <param name="sfx">音效音频流</param>
	public void PlaySfx(AudioStream sfx)
	{
		// 查找空闲的音效播放器
		foreach (var child in GetChildren())
		{
			if (child is AudioStreamPlayer player && player.Name.ToString().StartsWith("SfxPlayer") && !player.Playing)
			{
				player.Stream = sfx;
				player.VolumeDb = LinearToDb(SfxVolume * MasterVolume);
				player.Play();
				return;
			}
		}

		// 无空闲播放器时，使用主音效播放器（会中断当前音效）
		_sfxPlayer.Stream = sfx;
		_sfxPlayer.VolumeDb = LinearToDb(SfxVolume * MasterVolume);
		_sfxPlayer.Play();
	}

	/// <summary>在世界坐标位置播放音效（当前为全局播放，可扩展为 2D 音效）</summary>
	/// <param name="sfx">音效音频流</param>
	/// <param name="position">世界坐标位置</param>
	public void PlaySfxAtPosition(AudioStream sfx, Vector2 position)
	{
		PlaySfx(sfx);
	}

	/// <summary>设置主音量</summary>
	/// <param name="volume">音量值（0~1）</param>
	public void SetMasterVolume(float volume)
	{
		MasterVolume = Mathf.Clamp(volume, 0, 1);
		UpdateVolumes();
	}

	/// <summary>设置音效音量</summary>
	/// <param name="volume">音量值（0~1）</param>
	public void SetSfxVolume(float volume)
	{
		SfxVolume = Mathf.Clamp(volume, 0, 1);
		UpdateVolumes();
	}

	/// <summary>设置音乐音量</summary>
	/// <param name="volume">音量值（0~1）</param>
	public void SetMusicVolume(float volume)
	{
		MusicVolume = Mathf.Clamp(volume, 0, 1);
		UpdateVolumes();
	}

	// 更新所有音频播放器的音量
	private void UpdateVolumes()
	{
		_musicPlayer.VolumeDb = LinearToDb(MusicVolume * MasterVolume);
		_sfxPlayer.VolumeDb = LinearToDb(SfxVolume * MasterVolume);
		foreach (var child in GetChildren())
		{
			if (child is AudioStreamPlayer player && player.Name.ToString().StartsWith("SfxPlayer"))
			{
				player.VolumeDb = LinearToDb(SfxVolume * MasterVolume);
			}
		}
	}

	// 线性值转分贝值
	private static float LinearToDb(float linear)
	{
		return linear > 0 ? MathF.Log10(linear) * 20 : -80;
	}
}
