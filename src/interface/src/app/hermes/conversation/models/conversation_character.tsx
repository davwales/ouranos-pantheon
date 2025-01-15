import { CharacterDetail } from "@/gql/graphql";

export default interface ConversationCharacter {
    id?: string;
    name: string;
    age: number;
    details: CharacterDetail[];
};