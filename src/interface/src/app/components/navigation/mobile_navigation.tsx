"use client";

import List from '@/app/components/core/data-display/list';
import ListItemButton from '@/app/components/core/data-display/list_item_button';
import ListItemText from '@/app/components/core/data-display/list_item_text';
import ExpandLessIcon from '@/app/components/core/icons/expand_less_icon';
import ExpandMoreIcon from '@/app/components/core/icons/expand_more_icon';
import MenuIcon from '@/app/components/core/icons/menu_icon';
import IconButton from '@/app/components/core/inputs/icon_button';
import Box from '@/app/components/core/layout/box';
import Drawer from '@/app/components/core/navigation/drawer';
import Collapse from '@/app/components/core/utils/collapse';
import Link from 'next/link';
import React, { useState } from 'react';

export default function MobileNavigation() {
    const [drawerOpen, setDrawerOpen] = useState(false);
    const [isPlutusOpen, setPlutusOpen] = useState(false);
    const [isHermesOpen, setHermesOpen] = useState(false);

    const toggleDrawer =
        (open: boolean) => (event: React.KeyboardEvent | React.MouseEvent) => {
            if (
                event.type === 'keydown' &&
                ((event as React.KeyboardEvent).key === 'Tab' ||
                    (event as React.KeyboardEvent).key === 'Shift')
            ) {
                return;
            }
            setDrawerOpen(open);
        };

    const handleHermesClick = () => {
        setHermesOpen(!isHermesOpen);
    };

    const singleItem = (label: string, href: string) => (
        <Link href={href} passHref legacyBehavior>
            <ListItemButton component="a" onClick={toggleDrawer(false)}>
                <ListItemText primary={label} />
            </ListItemButton>
        </Link>
    );

    const multiItem = (
        label: string,
        setOpen: (isOpen: boolean) => void,
        isOpen: boolean,
        options: {
            label: string;
            href: string;
        }[]
    ) => (
        <>
            <ListItemButton onClick={() => setOpen(!isOpen)}>
                <ListItemText primary={label} />
                {isOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
            </ListItemButton>
            <Collapse in={isOpen}>
                <List disablePadding>
                    {options.map(o => (
                        <Link href={o.href} passHref legacyBehavior>
                            <ListItemButton
                                component="a"
                                onClick={toggleDrawer(false)}
                                styling={{ pl: 'large' }}
                            >
                                <ListItemText primary={o.label} />
                            </ListItemButton>
                        </Link>
                    ))}
                </List>
            </Collapse>
        </>
    );

    const homeItem = singleItem("Home", "/");

    const plutusItem = multiItem("Plutus", setPlutusOpen, isPlutusOpen, [
        {
            label: "Explorer",
            href: "/plutus/explorer"
        }
    ]);

    const hermesItem = multiItem("Hermes", setHermesOpen, isHermesOpen, [
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
        <>
            <IconButton
                color="inherit"
                edge="start"
                onClick={toggleDrawer(true)}
                styling={{ mr: 'large' }}
            >
                <MenuIcon />
            </IconButton>
            <Drawer anchor="left" open={drawerOpen} onClose={toggleDrawer(false)}>
                <Box
                    styling={{ width: 250 }}
                    role="presentation"
                    onKeyDown={toggleDrawer(false)}
                >
                    <List component="nav">
                        {homeItem}
                        {plutusItem}
                        {hermesItem}
                    </List>
                </Box>
            </Drawer>
        </>
    );
};
