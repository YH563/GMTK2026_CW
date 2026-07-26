using Godot;
using System;
using GameTemplate.Player;

namespace GameTemplate.Tricks;

public partial class DeadWall : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player.Player)
        {
            var player = (Player.Player)body;
            player.Remake();
        }
    }
}
