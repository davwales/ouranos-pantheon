import { CardActionArea as MuiCardActionArea } from "@mui/material";

interface CardActionAreaProps {
    children: React.ReactNode;
}

export default function CardActionArea(props: CardActionAreaProps) {
    return (
        <MuiCardActionArea>
            {props.children}
        </MuiCardActionArea>
    );
}