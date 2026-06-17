import {
  createRootRoute,
  createRoute,
  createRouter,
  redirect,
} from "@tanstack/react-router";
import { GameView } from "../components/GameView";
import { LoginPage } from "../pages/LoginPage";
import { RegisterPage } from "../pages/RegisterPage";
import { useAuthStore } from "../store/authStore";
import { RootLayout } from "../layouts/RootLayout";

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
  component: GameView,
});

const loginRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/login",
  beforeLoad: () => {
    if (useAuthStore.getState().token) {
      throw redirect({ to: "/" });
    }
  },
  component: LoginPage,
});

const registerRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/register",
  beforeLoad: () => {
    if (useAuthStore.getState().token) {
      throw redirect({ to: "/" });
    }
  },
  component: RegisterPage,
});

const routeTree = rootRoute.addChildren([
  indexRoute,
  loginRoute,
  registerRoute,
]);

export const router = createRouter({ routeTree });

declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}
