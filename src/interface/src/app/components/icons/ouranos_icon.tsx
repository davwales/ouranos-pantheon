import Avatar from "@/app/components/core/data-display/avatar";
import { StyleProps } from "@/app/components/core/style_props";

interface OuranosIconProps {
    iconSize?: number,
    styling?: StyleProps
};

export default function OuranosIcon(props: OuranosIconProps) {
    return (
        <Avatar
            alt="Ouranos"
            src="/ouranos_icon.png"
            styling={{
                ...props.styling,
                width: props.iconSize || 50,
                height: props.iconSize || 50
            }}
        />
    );
};