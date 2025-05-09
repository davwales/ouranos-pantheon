import InfoCard from "@/app/components/info-card";
import Link from "next/link";


export default function Hermes() {
    const modules = [
        {
            name: "Create Conversation",
            description: "Create a new conversation with a character.",
            href: "/hermes/conversation"
        },
        {
            name: "Manage Characters",
            description: "Manage your characters.",
            href: "/hermes/characters"
        }
    ];

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 m-4">
            {modules.map((module, index) => (
                <Link href={module.href} key={index} >
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