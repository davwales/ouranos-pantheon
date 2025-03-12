"use client";

import ExpandMoreIcon from '@/app/components/core/icons/expand_more_icon';
import Button from '@/app/components/core/inputs/button';
import Box from '@/app/components/core/layout/box';
import Menu from '@/app/components/core/navigation/menu';
import MenuItem from '@/app/components/core/navigation/menu_item';
import Link from 'next/link';
import { useState } from 'react';

export default function DesktopNavigation() {
    const [hermesAnchor, setHermesAnchor] = useState<HTMLElement | undefined>();
    const [plutusAnchor, setPlutusAnchor] = useState<HTMLElement | undefined>();

    const singleItem = (label: string, href: string) => (
        <Link href={href} passHref legacyBehavior>
            <Button color="inherit" component="a">
                {label}
            </Button>
        </Link>
    );

    const multiItem = (
        label: string,
        anchorEl: HTMLElement | undefined,
        setAnchorEl: (x: HTMLElement | undefined) => void,
        options: {
            label: string;
            href: string;
        }[]
    ) => (
        <>
            <Button
                color="inherit"
                onClick={(e) => setAnchorEl(e.currentTarget)}
                endIcon={<ExpandMoreIcon />}
            >
                {label}
            </Button>
            <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={() => setAnchorEl(undefined)}
            >
                {options.map(o => (
                    <Link href={o.href} passHref legacyBehavior>
                        <MenuItem onClick={() => setAnchorEl(undefined)} component="a">
                            {o.label}
                        </MenuItem>
                    </Link>
                ))}
            </Menu>
        </>
    );

    const homeItem = singleItem("Home", "/");

    const plutusItem = multiItem("Plutus", plutusAnchor, setPlutusAnchor, [
        {
            label: "Explorer",
            href: "/plutus/explorer"
        }
    ]);

    const hermesItem = multiItem("Hermes", hermesAnchor, setHermesAnchor, [
        {
            label: "Create Conversation",
            href: "/hermes/conversation"
        },
        {
            label: "Manage Characters",
            href: "/hermes/characters"
        }
    ]);

    return (
        <Box styling={{ display: "flex", alignItems: "center" }}>
            {homeItem}
            {hermesItem}
            {plutusItem}
        </Box>
    );
};
