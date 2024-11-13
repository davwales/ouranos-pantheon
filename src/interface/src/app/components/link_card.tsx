"use client";

import React from 'react';
import { Card, CardActionArea, SxProps, useTheme } from '@mui/material';
import Link from 'next/link';

interface LinkCardProps {
    href: string;
    children: React.ReactNode;
    linkStyle?: React.CSSProperties;
    sxCard?: SxProps;
}

export default function LinkCard(props: LinkCardProps) {
    const theme = useTheme();

    return (
        <Link href={props.href} style={{ ...props.linkStyle, textDecoration: "none" }}>
            <Card
                variant="outlined"
                sx={{
                    ...props.sxCard,
                }}
            >
                <CardActionArea>
                    {props.children}
                </CardActionArea>
            </Card>
        </Link>
    );
};
