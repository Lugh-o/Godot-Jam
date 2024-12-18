using Godot;
using System;

public partial class Enemy : Damageable
{
	[Export] public float walkSpeed = 4.3f;
	[Export] public float attackRange = 5f;
	[Export] public double damage = 1;

	[Export] public NavigationAgent3D navigationAgent;
	[Export] public Level01 level;
	[Export] public Sprite3D sprite;
	[Export] public Timer surviveTimer;

	[Export] public double healingDropRate = 0.2;
	[Export] public double ammoDropRate = 0.2;

	public PackedScene healingDrop = ResourceLoader.Load<PackedScene>("res://Scenes/Drops/HealingDrop/HealingDrop.tscn");
	public PackedScene ammoDrop = ResourceLoader.Load<PackedScene>("res://Scenes/Drops/AmmoDrop/AmmoDrop.tscn");

	public int navigationFrameThreshold = 40;
	public int currentNavigationFrame = 0;
	public int facingFrameThreshold = 20;
	public int currentFacingFrame = 0;
	[Export] public int group;

	public override void _PhysicsProcess(double delta)
	{
		HandleFacing();
		HandleMovement((float)delta);
		MoveAndSlide();
	}

	public bool TargetInRange()
	{
		return GlobalPosition.DistanceTo(player.GlobalPosition) < attackRange;
	}

	public void HandleMovement(float deltaFloat)
	{
		currentNavigationFrame++;
		if (currentNavigationFrame >= navigationFrameThreshold + group)
		{
			if (!IsOnFloor()) Velocity += GetGravity() * deltaFloat;

			currentNavigationFrame = 0;
			if (TargetInRange())
			{
				Velocity = new Vector3(GD.RandRange(-2, 2), 0, 0);
				HandleAttack();
			}
			else
			{
				navigationAgent.TargetPosition = player.GlobalTransform.Origin;
				Vector3 nextNavigationPosition = navigationAgent.GetNextPathPosition() + new Vector3(GD.RandRange(-1, 1), 0, 0);
				Velocity = (nextNavigationPosition - GlobalTransform.Origin).Normalized() * walkSpeed;
			}
		}
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

	public virtual void HandleAttack() { }

	public void DropHealing()
	{
		HealingDrop healingDropInstance = healingDrop.Instantiate<HealingDrop>();
		level.AddChild(healingDropInstance);
		healingDropInstance.GlobalPosition = GlobalPosition;
		healingDropInstance.player = player;
	}

	public void DropAmmo()
	{
		AmmoDrop ammoDropInstance = ammoDrop.Instantiate<AmmoDrop>();
		level.AddChild(ammoDropInstance);
		ammoDropInstance.GlobalPosition = GlobalPosition;
		ammoDropInstance.player = player;
	}

	public override void Die()
	{
		float rng = GD.Randf();



		if (rng <= healingDropRate)
		{
			DropHealing();
		}
		else if (rng < healingDropRate + ammoDropRate)
		{
			DropAmmo();
		}

		player.GainExperience(5);
		QueueFree();
	}

	public void _onScreenEntered()
	{
		sprite.Show();
	}

	public void _onScreenExited()
	{
		sprite.Hide();
	}

	public void HandleScaling()
	{
		MaxHealth *= 1 + ((surviveTimer.TimeLeft - surviveTimer.WaitTime) * (-0.01));
		damage *= 1 + (surviveTimer.TimeLeft - surviveTimer.WaitTime) * (-0.01);
	}

}
