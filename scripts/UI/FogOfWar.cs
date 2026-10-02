using Godot;
using System;

// FOW for player onlym, no visual data 'should' get sent across network to other clientss
public partial class FogOfWar : Node2D
{
	[Export] public float viewRadius = 250.0f;
	[Export] public Vector2 worldMin = new Vector2(-2000, -2000);
	[Export] public Vector2 worldSize = new Vector2(24000, 14000);
	
	[Export] public Vector2I maskResolution = new Vector2I(1200, 700);

	public Player player;
	public Camera2D camera;

	private SubViewport maskViewport;
	private Sprite2D revealSprite;
	private ColorRect fogRect;
	private ShaderMaterial fogMaterial;
	private GradientTexture2D revealTexture;
	private CanvasLayer fogLayer;

	Shader fogShader = GD.Load<Shader>("res://scenes/UI/FogOfWar.gdshader");

	public override void _Ready()
	{
		//white blob stamps where player goes, done in code so no image needs doing this is the best way to do it apparentlyg
		Gradient gradient = new Gradient();
		gradient.SetColor(0, new Color(1, 1, 1, 1));
		gradient.SetColor(1, new Color(1, 1, 1, 0));
		revealTexture = new GradientTexture2D();
		revealTexture.Gradient = gradient;
		revealTexture.Fill = GradientTexture2D.FillEnum.Radial;
		revealTexture.FillFrom = new Vector2(0.5f, 0.5f);
		revealTexture.FillTo = new Vector2(0.5f, 0.0f);
		revealTexture.Width = 256;
		revealTexture.Height = 256;

		//explored map once stamped with white blob stays around, i.e explored
		maskViewport = new SubViewport();
		maskViewport.Size = maskResolution;
		maskViewport.RenderTargetClearMode = SubViewport.ClearMode.Never;
		maskViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
		maskViewport.TransparentBg = true;
		AddChild(maskViewport);

		revealSprite = new Sprite2D();
		revealSprite.Texture = revealTexture;
		maskViewport.AddChild(revealSprite);

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
		fogMaterial.SetShaderParameter("exploredMask", maskViewport.GetTexture());
		fogMaterial.SetShaderParameter("worldMin", worldMin);
		fogMaterial.SetShaderParameter("worldSize", worldSize);
		fogRect.Material = fogMaterial;

		GD.Print("FogOfWar ready for player " + player.playerID);
	}

	public override void _Process(double delta)
	{
		//blob moves with player here
		Vector2 maskScale = new Vector2(maskResolution.X, maskResolution.Y) / worldSize;
		Vector2 maskPos = (player.GlobalPosition - worldMin) * maskScale;
		revealSprite.Position = maskPos;
		//blob same size as view radius
		float texRadius = revealTexture.GetSize().X * 0.5f;
		revealSprite.Scale = (Vector2.One * viewRadius * maskScale.X) / texRadius;

		//shows where the camera is to line up with mask shader, terminology probably shit
		fogMaterial.SetShaderParameter("cameraWorldPos", camera.GetScreenCenterPosition());
		fogMaterial.SetShaderParameter("viewportSize", GetViewport().GetVisibleRect().Size);
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
