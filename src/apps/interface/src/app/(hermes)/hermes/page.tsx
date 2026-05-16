import { InfoCard } from "@/components/shared/info-card";
import Link from "next/link";

export default function Hermes() {
  const modules = [
    {
      name: "Create Conversation",
      description: "Start a new conversation with a persona and model.",
      href: "/hermes/chat",
    },
    {
      name: "Saved Conversations",
      description: "View and resume your saved conversations.",
      href: "/hermes/conversations",
    },
    {
      name: "Manage Personas",
      description: "Create and manage character personas.",
      href: "/hermes/personas",
    },
    {
      name: "Manage Models",
      description: "Configure LLM models and their parameters.",
      href: "/hermes/models",
    },
    {
      name: "Manage Traits",
      description: "Create and manage conversation traits.",
      href: "/hermes/traits",
    },
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 m-4">
      {modules.map((module, index) => (
        <Link href={module.href} key={index}>
          <InfoCard
            label={module.name}
            description={module.description}
            className="hover:bg-accent h-full w-full"
          />
        </Link>
      ))}
    </div>
  );
}
