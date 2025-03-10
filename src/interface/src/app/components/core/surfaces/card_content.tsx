import { CardContent as MuiCartContent } from "@mui/material";

interface CardContentProps {
    children: React.ReactNode;
}

export default function CardContent(props: CardContentProps) {
    return (
        <MuiCartContent>
            {props.children}
        </MuiCartContent>
    );
}
