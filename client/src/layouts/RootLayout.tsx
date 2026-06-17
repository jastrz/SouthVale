import { Outlet } from "@tanstack/react-router";
import { LoginBar } from "../components/LoginBar";

export function RootLayout() {
  return (
    <>
      <LoginBar />
      <Outlet />
    </>
  );
}
