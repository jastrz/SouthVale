import { Filter, GlProgram, UniformGroup } from "pixi.js";

// Wind sway for tree/bush sprites.
const VERTEX = `
#version 300 es
in vec2 aPosition;
out vec2 vTextureCoord;

uniform vec4 uInputSize;
uniform vec4 uOutputFrame;
uniform vec4 uOutputTexture;
uniform float uTime;
uniform float uPhase;
uniform float uAmp;
uniform float uFreq;

vec4 filterVertexPosition(void) {
    vec2 position = aPosition * uOutputFrame.zw + uOutputFrame.xy;
    // aPosition.y doubles as height: base stays planted, top sways
    position.x += sin(uTime * uFreq + uPhase) * uAmp * (1.0 - aPosition.y);
    position.x = position.x * (2.0 / uOutputTexture.x) - 1.0;
    position.y = position.y * (2.0 * uOutputTexture.z / uOutputTexture.y) - uOutputTexture.z;
    return vec4(position, 0.0, 1.0);
}

vec2 filterTextureCoord(void) {
    return aPosition * (uOutputFrame.zw * uInputSize.zw);
}

void main(void) {
    gl_Position = filterVertexPosition();
    vTextureCoord = filterTextureCoord();
}
`;

const FRAGMENT = `
#version 300 es
in vec2 vTextureCoord;
out vec4 outColor;
uniform sampler2D uTexture;

void main(void) {
    outColor = texture(uTexture, vTextureCoord);
}
`;

const program = new GlProgram({ vertex: VERTEX, fragment: FRAGMENT });

const liveFilters: Filter[] = [];

export interface TreeSwayOptions {
  phase?: number;
  amp?: number;
  freq?: number;
}

export function treeSwayFilter({
  phase = Math.random() * Math.PI * 2,
  amp = 1.5,
  freq = 1.8,
}: TreeSwayOptions = {}): Filter {
  const filter = new Filter({
    glProgram: program,
    resources: {
      uniforms: {
        uTime: { value: 0, type: "f32" },
        uPhase: { value: phase, type: "f32" },
        uAmp: { value: amp, type: "f32" },
        uFreq: { value: freq, type: "f32" },
      },
    },
  });
  liveFilters.push(filter);
  return filter;
}

export function updateTreeSway(dtSeconds: number): void {
  for (const f of liveFilters) {
    const uniforms = (f.resources.uniforms as UniformGroup).uniforms;
    uniforms.uTime = (uniforms.uTime as number) + dtSeconds;
  }
}
