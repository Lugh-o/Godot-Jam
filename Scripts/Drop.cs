using Godot;
using System;

public partial class Drop : CharacterBody3D
{
    public Player player;
    private Timer _lifetimeTimer;
    public int facingFrameThreshold = 10;
    public int currentFacingFrame = 0;
    [Export] public Sprite3D sprite;

    private bool picked = false;

    public override void _Ready()
    {
        _lifetimeTimer = new Timer();
        _lifetimeTimer.WaitTime = 30.0f;
        _lifetimeTimer.OneShot = true;
        _lifetimeTimer.Timeout += OnTimerTimeout;
        AddChild(_lifetimeTimer);
        _lifetimeTimer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleFacing();
        if (!IsOnFloor()) Velocity += GetGravity() * (float)delta;
        MoveAndSlide();

        if (picked) Velocity += GlobalPosition.DirectionTo(player.GlobalPosition) * 5f;
    }

    public void HandleFacing()
    {
        currentFacingFrame++;
        if (currentFacingFrame >= facingFrameThreshold)
        {
            Vector3 playerPosition = new(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z);
            LookAt(playerPosition, Vector3.Up);
        }
    }

    private void OnTimerTimeout()
    {
        QueueFree();
    }

    public void GoToPlayer()
    {
        picked = true;
    }

    public void _onScreenEntered() {
        sprite.Show();
    }

    public void _onScreenExited() {
        sprite.Hide();
    }

}
