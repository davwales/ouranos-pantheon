import { StyleProps } from '@/app/components/core/style_props';
import Card from '@/app/components/core/surfaces/card';
import CardActionArea from '@/app/components/core/surfaces/card_action_area';
import Link from 'next/link';
import React from 'react';

interface LinkCardProps {
    href: string;
    children: React.ReactNode;
    linkStyle?: React.CSSProperties;
    cardStyling?: StyleProps;
}

export default function LinkCard(props: LinkCardProps) {
    return (
        <Link href={props.href} style={{ ...props.linkStyle, textDecoration: "none" }}>
            <Card
                variant="outlined"
                styling={props.cardStyling}
            >
                <CardActionArea>
                    {props.children}
                </CardActionArea>
            </Card>
        </Link>
    );
};
