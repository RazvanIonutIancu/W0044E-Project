using Godot;
using System;

// FOW for player onlym, no visual data 'should' get sent across network to other clientss
public partial class FogOfWar : Node2D
{
	//how long thes cone is, currently just made it massive because you wanted true LoS
	[Export] public float coneLength = 1200.0f;
	//how wide the cone opens up, in degrees, fov, maybe 60 is a bit too big
	[Export] public float coneAngle = 60.0f;
	//more rays = smoother cone edge but a bit more work each frame
	[Export] public int coneRays = 20;
	//little circle of vision around the player so you can see stuff right next to you, this penetrates walls currently 
	[Export] public float sightRadius = 250.0f;

	[Export] public Vector2 worldMin = new Vector2(-2000, -2000);
	[Export] public Vector2 worldSize = new Vector2(24000, 14000);

	//bigger numbers = sharper fog edges but a bigger texture to maintrain
	[Export] public Vector2I maskResolution = new Vector2I(2400, 1400);

	public Player player;
	public Camera2D camera;

	private SubViewport exploredViewport;
	private SubViewport visibleViewport;
	private Polygon2D exploredCone;
	private Polygon2D visibleCone;
	private Sprite2D exploredBlob;
	private Sprite2D visibleBlob;
	private GradientTexture2D blobTexture;
	private ColorRect fogRect;
	private ShaderMaterial fogMaterial;
	private CanvasLayer fogLayer;

	// walls are on physics layer 5, which is 16 as a mask
	uint wallMask = 16;

	Shader fogShader = GD.Load<Shader>("res://scenes/UI/FogOfWar.gdshader");

	public override void _Ready()
	{
		//round white blob we stamp around the player for the aoe vision circle
		Gradient gradient = new Gradient();
		gradient.SetColor(0, new Color(1, 1, 1, 1));
		gradient.SetColor(1, new Color(1, 1, 1, 0));
		blobTexture = new GradientTexture2D();
		blobTexture.Gradient = gradient;
		blobTexture.Fill = GradientTexture2D.FillEnum.Radial;
		blobTexture.FillFrom = new Vector2(0.5f, 0.5f);
		blobTexture.FillTo = new Vector2(0.5f, 0.0f);
		blobTexture.Width = 256;
		blobTexture.Height = 256;

		//explored map once stamped stays around, but it is slightly dimemr
		exploredViewport = new SubViewport();
		exploredViewport.Size = maskResolution;
		exploredViewport.RenderTargetClearMode = SubViewport.ClearMode.Never;
		exploredViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
		exploredViewport.TransparentBg = true;
		AddChild(exploredViewport);

		exploredCone = new Polygon2D();
		exploredCone.Color = new Color(1, 1, 1, 1);
		exploredViewport.AddChild(exploredCone);

		exploredBlob = new Sprite2D();
		exploredBlob.Texture = blobTexture;
		exploredViewport.AddChild(exploredBlob);

		//live map clears every frame, only shows what the cone can see right now
		visibleViewport = new SubViewport();
		visibleViewport.Size = maskResolution;
		visibleViewport.RenderTargetClearMode = SubViewport.ClearMode.Always;
		visibleViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
		visibleViewport.TransparentBg = true;
		AddChild(visibleViewport);

		visibleCone = new Polygon2D();
		visibleCone.Color = new Color(1, 1, 1, 1);
		visibleViewport.AddChild(visibleCone);

		visibleBlob = new Sprite2D();
		visibleBlob.Texture = blobTexture;
		visibleViewport.AddChild(visibleBlob);

		//black overlay dog
		fogLayer = new CanvasLayer();
		fogLayer.Layer = 10;
		AddChild(fogLayer);

		fogRect = new ColorRect();
		fogRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		fogRect.MouseFilter = Control.MouseFilterEnum.Ignore;
		fogLayer.AddChild(fogRect);

		fogMaterial = new ShaderMaterial();
		fogMaterial.Shader = fogShader;
		fogMaterial.SetShaderParameter("exploredMask", exploredViewport.GetTexture());
		fogMaterial.SetShaderParameter("visibleMask", visibleViewport.GetTexture());
		fogMaterial.SetShaderParameter("worldMin", worldMin);
		fogMaterial.SetShaderParameter("worldSize", worldSize);
		fogRect.Material = fogMaterial;

		GD.Print("FogOfWar ready for player " + player.playerID);
	}

	public override void _Process(double delta)
	{
		//build the cone points, stop each ray where it hits a wall
		Vector2[] conePoints = BuildConePoints();

		//shrink the cone from world size down to mask size so it fits on the maps
		Vector2 maskScale = new Vector2(maskResolution.X, maskResolution.Y) / worldSize;
		Vector2[] maskPoints = new Vector2[conePoints.Length];
		for(int i = 0; i < conePoints.Length; i++)
		{
			maskPoints[i] = (conePoints[i] - worldMin) * maskScale;
		}
		exploredCone.Polygon = maskPoints;
		visibleCone.Polygon = maskPoints;

		//sit the little vision circle blob on the player and size it to the sight radius
		Vector2 playerMaskPos = (player.GlobalPosition - worldMin) * maskScale;
		float blobRadius = blobTexture.GetSize().X * 0.5f;
		Vector2 blobScale = (Vector2.One * sightRadius * maskScale.X) / blobRadius;
		exploredBlob.Position = playerMaskPos;
		exploredBlob.Scale = blobScale;
		visibleBlob.Position = playerMaskPos;
		visibleBlob.Scale = blobScale;

		//show any clones and players we can actually see right now
		UpdateEntityVisibility();

		//shows where the camera is to line up with mask shader, terminology probably shit
		fogMaterial.SetShaderParameter("cameraWorldPos", camera.GetScreenCenterPosition());
		fogMaterial.SetShaderParameter("viewportSize", GetViewport().GetVisibleRect().Size);
	}

	//shoots a bunch of rays out in a fan to make the cone shape, each ray stops when it hits a wall
	private Vector2[] BuildConePoints()
	{
		Vector2 origin = player.GlobalPosition;
		float halfAngle = Mathf.DegToRad(coneAngle) * 0.5f;
		//angle gap between one ray and the next so it looks like one big cone isntead of loads of rays
		float step = Mathf.DegToRad(coneAngle) / (coneRays - 1);

		//first point is the player, the rest fan out in front
		Vector2[] points = new Vector2[coneRays + 1];
		points[0] = origin;

		var space = GetWorld2D().DirectSpaceState;
		for(int i = 0; i < coneRays; i++)
		{
			//start at the left edge of the cone and sweep across to the right
			float angle = player.Rotation - halfAngle + (step * i);
			Vector2 target = origin + Vector2.FromAngle(angle) * coneLength;

			var query = PhysicsRayQueryParameters2D.Create(origin, target, wallMask);
			var hit = space.IntersectRay(query);
			if(hit.Count > 0)
			{
				//hit a wall, stop the ray there
				points[i + 1] = (Vector2)hit["position"];
			}
			else
			{
				//nothing in the way, ray goes all the way out
				points[i + 1] = target;
			}
		}
		return points;
	}

	private void UpdateEntityVisibility()
	{
		foreach(Player other in GameManager.Instance().playerNodeList)
		{
			if(other == player) { continue; }
			//dead players stay hidden, they left a clone behind anyway
			other.GetNode<Polygon2D>("Mesh").Visible = other.isAlive && CanSee(other.GlobalPosition);
		}

		foreach(Node clone in GetTree().GetNodesInGroup("clones"))
		{
			Node2D clone2D = (Node2D)clone;
			clone.GetNode<Polygon2D>("Mesh").Visible = CanSee(clone2D.GlobalPosition);
		}
	}

	//true if the thing is close enough, in front of us inside the cone, and not behind a wall
	private bool CanSee(Vector2 targetPos)
	{
		Vector2 origin = player.GlobalPosition;
		Vector2 toTarget = targetPos - origin;

		//right next to us, always visible even behind us or through a wall, balacing choice we have to make about seeing behind wall tho
		if(toTarget.Length() <= sightRadius) { return true; }

		//too far away.. basicalyl never gonna happen lol
		if(toTarget.Length() > coneLength) { return false; }

		//outside the cone's angle
		float halfAngle = Mathf.DegToRad(coneAngle) * 0.5f;
		float angleToTarget = Mathf.Abs(Mathf.AngleDifference(player.Rotation, toTarget.Angle()));
		if(angleToTarget > halfAngle) { return false; }

		//wall in the way?
		var space = GetWorld2D().DirectSpaceState;
		var query = PhysicsRayQueryParameters2D.Create(origin, targetPos, wallMask);
		var hit = space.IntersectRay(query);
		return hit.Count == 0;
	}

	// DEBUG

	public override void _Input(InputEvent @event)
	{
		//press U to hide/show the fog, explored memory still builds up underneath
		if(@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.U)
		{
			fogLayer.Visible = !fogLayer.Visible;
			GD.Print("Fog of war toggled: " + fogLayer.Visible);
		}
	}

	// DEBUG END
}
