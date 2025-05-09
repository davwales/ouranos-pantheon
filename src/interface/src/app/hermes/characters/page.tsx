"use client";

import InfoCard from "@/app/components/info-card";
import { GET_CHARACTER_LIST } from "@/app/hermes/queries";
import { Button } from "@/components/ui/button";
import { useQuery } from "@urql/next";
import Link from "next/link";

export default function CharactersPage() {
    const [{ data }] = useQuery({ query: GET_CHARACTER_LIST })

    return (
        <div className="m-4">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {data?.allCharacters.map((character) => (
                    <Link href={`/hermes/characters/${character.id}`} key={character.id} >
                        <InfoCard
                            label={character.name}
                            description={`Age: ${character.age}`}
                            className="hover:bg-accent h-full w-full"
                        />
                    </Link>
                ))}
            </div>

            <Button size="lg" variant="link" className="mt-4 w-full">
                <Link href="/hermes/characters/create">
                    Create New
                </Link>
            </Button>
        </div>
    );
}