import { resolveSpacing } from "@/app/components/core/mui_style_resolvers";
import { SpacingToken } from "@/app/components/core/style_props";
import { Stack as MuiStack } from "@mui/material";

interface StackProps {
    children: React.ReactNode;
    spacing: SpacingToken;
}

export default function Stack(props: StackProps) {
    return (
        <MuiStack spacing={resolveSpacing(props.spacing)}>
            {props.children}
        </MuiStack>
    );
}
