import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { Avatar as MuiAvatar } from "@mui/material";

interface AvatarProps {
    alt: string,
    src: string,
    styling?: StyleProps
};

export default function Avatar(props: AvatarProps) {
    return (
        <MuiAvatar
            alt={props.alt}
            src={props.src}
            sx={props.styling && convertToSx(props.styling)}
        />
    );
}