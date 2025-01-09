import { Role } from "@/gql/graphql";

export default interface Message {
    role: Role;
    content: string;
};
