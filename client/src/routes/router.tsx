import {
  createRootRoute,
  createRoute,
  createRouter,
  redirect,
} from "@tanstack/react-router";
import { GamePage } from "../pages/GamePage";
import { LoginPage } from "../pages/LoginPage";
import { RegisterPage } from "../pages/RegisterPage";
import { useAuthStore } from "../store/authStore";
import { RootLayout } from "../layouts/RootLayout";
import { FrontpageLayout } from "../layouts/FrontpageLayout";
import { MapEditorPage } from "../pages/MapEditorPage";
import { AdminPage } from "../pages/AdminPage";
import { jwtRole } from "../lib/helpers";

const rootRoute = createRootRoute({
  component: RootLayout,
});

const indexRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/",
  beforeLoad: () => {
    if (!useAuthStore.getState().token) {
      throw redirect({ to: "/login" });
    }
  },
  component: GamePage,
});

const frontpageRoute = createRoute({
  getParentRoute: () => rootRoute,
  id: "frontpage",
  component: FrontpageLayout,
});

const loginRoute = createRoute({
  getParentRoute: () => frontpageRoute,
  path: "/login",
  beforeLoad: () => {
    if (useAuthStore.getState().token) {
      throw redirect({ to: "/" });
    }
  },
  component: LoginPage,
});

const registerRoute = createRoute({
  getParentRoute: () => frontpageRoute,
  path: "/register",
  beforeLoad: () => {
    if (useAuthStore.getState().token) {
      throw redirect({ to: "/" });
    }
  },
  component: RegisterPage,
});

const mapEditorRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/map-editor",
  component: MapEditorPage,
});

const adminRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/admin",
  beforeLoad: () => {
    const token = useAuthStore.getState().token;
    if (!token) throw redirect({ to: "/login" });
    if (jwtRole(token) !== "Admin") throw redirect({ to: "/" });
  },
  component: AdminPage,
});

const children = [
  indexRoute,
  frontpageRoute.addChildren([loginRoute, registerRoute]),
  mapEditorRoute,
  adminRoute,
];

const routeTree = rootRoute.addChildren(children);

export const router = createRouter({ routeTree, basepath: import.meta.env.BASE_URL.replace(/\/$/, "") });

declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}
