shader_type canvas_item;

//the explored map the subviewport keeps drawing into, bright bits = seen before
uniform sampler2D exploredMask;

//stuff needed to work out which mask pixel a screen pixel maps to
uniform vec2 cameraWorldPos;
uniform vec2 viewportSize;
uniform vec2 worldMin;
uniform vec2 worldSize;

//how dark a seen area goes once you walk away from it (0 = black, 1 = clear)
uniform float exploredDarkness : hint_range(0.0, 1.0) = 0.45;

void fragment() {
	//screen pixel -> world pos, camera sits in the middle of the screen
	vec2 screenPixel = SCREEN_UV * viewportSize;
	vec2 worldPos = cameraWorldPos + (screenPixel - viewportSize * 0.5);

	//world pos -> where that lands on the explored map
	vec2 maskUv = (worldPos - worldMin) / worldSize;

	float explored = 0.0;
	if (maskUv.x >= 0.0 && maskUv.x <= 1.0 && maskUv.y >= 0.0 && maskUv.y <= 1.0) {
		explored = texture(exploredMask, maskUv).r;
	}

	//never seen = solid black, seen before = a bit see-through
	float alpha = mix(1.0, 1.0 - exploredDarkness, clamp(explored, 0.0, 1.0));
	COLOR = vec4(0.0, 0.0, 0.0, alpha);
}
