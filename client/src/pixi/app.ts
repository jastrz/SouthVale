import { Application } from "pixi.js";
import { COLORS } from "./config";

/**
 * Creates a Pixi Application sized to the given container, appends its
 * canvas to the container, and returns the ready app.
 */
export async function createApplication(
  container: HTMLElement,
): Promise<Application> {
  const app = new Application();
  await app.init({ resizeTo: container, background: COLORS.background });
  container.appendChild(app.canvas);
  return app;
}
