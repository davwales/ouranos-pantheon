import InfoCard from '@/app/components/info-card';
import { CharacterForm } from '@/app/hermes/components/character_form';
import ConversationCharacter from '@/app/hermes/conversation/models/conversation_character';
import { GET_DETAILED_CHARACTER_LIST } from '@/app/hermes/queries';
import { Drawer, DrawerContent, DrawerDescription, DrawerHeader, DrawerTitle, DrawerTrigger } from '@/components/ui/drawer';
import { Role } from '@/gql/graphql';
import { useQuery } from '@urql/next';
import { useState } from 'react';

export default function SelectCharacterView({
    role,
    character,
    setCharacter,
    className,
    ...props
}: React.ComponentProps<"div"> & {
    role: Role;
    character?: ConversationCharacter | undefined;
    setCharacter: (character: ConversationCharacter | undefined) => void;
}) {
    const [drawerOpen, setDrawerOpen] = useState<boolean>(false);

    const [{ data }] = useQuery({
        query: GET_DETAILED_CHARACTER_LIST,
    });

    const handleCharacterModified = (modifiedCharacter: ConversationCharacter) => {
        setDrawerOpen(false);
        setCharacter(modifiedCharacter);
    };

    return (
        <div {...props} className={`mt-4 ${className}`}>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                {data?.allCharacters.map((c, characterIndex) => (
                    <InfoCard
                        key={characterIndex}
                        label={c.name}
                        description={`Age: ${c.age}`}
                        onClick={() => setCharacter(c == character ? undefined : c)}
                        className={`hover:bg-accent hover:cursor-pointer h-full w-full ${c == character ? "border-accent-foreground" : ""}`}
                    />
                ))}
            </div>

            {character && (
                <Drawer open={drawerOpen} onOpenChange={setDrawerOpen}>
                    <DrawerTrigger className="w-full mt-4 p-2 rounded hover:cursor-pointer hover:bg-accent/50">
                        Modify Character
                    </DrawerTrigger>
                    <DrawerContent>
                        <DrawerHeader>
                            <DrawerTitle>
                                Modifying {character.name}
                            </DrawerTitle>
                            <DrawerDescription>
                                Make slight adjustments to {character.name} for this specific conversation.
                            </DrawerDescription>
                        </DrawerHeader>
                        <CharacterForm
                            initialValues={character}
                            onSave={handleCharacterModified}
                            className="mx-4 px-2 pb-4 rounded overflow-scroll"
                        />
                    </DrawerContent>
                </Drawer>
            )}
        </div>
    );
};
