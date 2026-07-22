using Godot;
using System;

namespace GameTemplate.Utils;

public static class Extensions
{
	/// <summary>在视口范围内获取随机位置</summary>
	/// <param name="camera">摄像机实例</param>
	/// <param name="margin">边缘留白距离</param>
	/// <returns>视口内的随机坐标</returns>
	public static Vector2 GetRandomViewportPosition(this Camera2D camera, float margin = 50)
	{
		var viewRect = camera.GetViewportRect();
		return new Vector2(
			(float)GD.RandRange((double)(viewRect.Position.X + margin), (double)(viewRect.End.X - margin)),
			(float)GD.RandRange((double)(viewRect.Position.Y + margin), (double)(viewRect.End.Y - margin))
		);
	}

	/// <summary>使用 Fisher-Yates 算法原地打乱列表</summary>
	/// <typeparam name="T">列表元素类型</typeparam>
	/// <param name="list">要打乱的列表</param>
	public static void Shuffle<T>(this System.Collections.Generic.List<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = GD.RandRange(0, n);
			(list[k], list[n]) = (list[n], list[k]);
		}
	}

	/// <summary>从列表中随机取一个元素</summary>
	/// <typeparam name="T">列表元素类型</typeparam>
	/// <param name="list">源列表</param>
	/// <returns>随机元素，列表为空返回 default</returns>
	public static T RandomElement<T>(this System.Collections.Generic.List<T> list)
	{
		if (list.Count == 0)
			return default;
		return list[GD.RandRange(0, list.Count - 1)];
	}

	/// <summary>将数值包裹在 [min, max] 范围内（循环回绕）</summary>
	/// <param name="value">原始值</param>
	/// <param name="min">范围下限</param>
	/// <param name="max">范围上限</param>
	/// <returns>包裹后的值</returns>
	public static float Wrap(this float value, float min, float max)
	{
		float range = max - min;
		return ((value - min) % range + range) % range + min;
	}

	/// <summary>判断浮点数是否约等于零</summary>
	/// <param name="value">要判断的值</param>
	/// <param name="tolerance">允许的误差范围</param>
	/// <returns>是否近似为零</returns>
	public static bool IsZero(this float value, float tolerance = 0.001f)
	{
		return Mathf.Abs(value) < tolerance;
	}

	/// <summary>淡入显示控件</summary>
	/// <param name="control">目标控件</param>
	/// <param name="duration">淡入持续时间（秒）</param>
	public static void FadeIn(this Control control, float duration = 0.3f)
	{
		control.Modulate = new Color(1, 1, 1, 0);
		control.Visible = true;
		var tween = control.CreateTween();
		tween.TweenProperty(control, "modulate", new Color(1, 1, 1, 1), duration);
	}

	/// <summary>淡出隐藏控件</summary>
	/// <param name="control">目标控件</param>
	/// <param name="duration">淡出持续时间（秒）</param>
	public static void FadeOut(this Control control, float duration = 0.3f)
	{
		var tween = control.CreateTween();
		tween.TweenProperty(control, "modulate", new Color(1, 1, 1, 0), duration);
		tween.TweenCallback(Callable.From(() => control.Visible = false));
	}
}
