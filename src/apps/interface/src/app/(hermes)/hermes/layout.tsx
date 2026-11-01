import { PublicModeProvider } from "@/app/(hermes)/hermes/components/public-mode-context";

export default function HermesLayout({ children }: React.PropsWithChildren) {
  return <PublicModeProvider>{children}</PublicModeProvider>;
}
