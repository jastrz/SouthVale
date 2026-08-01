import { Application } from "pixi.js";
import {
  loadAtlas,
  loadGroundAtlas,
  loadTreesAtlas,
  loadVillageTextures,
  loadMovementIcons,
} from "./atlas";

/**
 * Creates a Pixi Application sized to the given container, appends its
 * canvas to the container, loads the tile atlases, and returns the ready app.
 */
export async function createApplication(
  container: HTMLElement,
): Promise<Application> {
  const app = new Application();
  // preference: "webgl" — the tree sway filter ships GLSL only; WebGPU
  // would need a matching WGSL program (passthrough default works, custom
  // uniforms don't).
  await app.init({ resizeTo: container, preference: "webgl" });
  container.appendChild(app.canvas);
  await Promise.all([
    loadAtlas("/tiles/tilesheet.png"),
    loadGroundAtlas("/tiles/ground.png"),
    loadTreesAtlas("/tiles/trees_all.png"),
    loadVillageTextures(),
    loadMovementIcons(),
  ]);
  return app;
}
