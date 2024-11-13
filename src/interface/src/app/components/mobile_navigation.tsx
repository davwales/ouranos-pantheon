// components/MobileNavigation.tsx
"use client";

import React, { useState } from 'react';
import {
    Drawer,
    IconButton,
    List,
    ListItemText,
    ListItemButton,
    Collapse,
    Box,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import { ExpandLess, ExpandMore } from '@mui/icons-material';
import Link from 'next/link';

export default function MobileNavigation() {
    const [drawerOpen, setDrawerOpen] = useState(false);
    const [aphroditeOpen, setAphroditeOpen] = useState(false);

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

    const handleAphroditeClick = () => {
        setAphroditeOpen(!aphroditeOpen);
    };

    return (
        <>
            <IconButton
                color="inherit"
                edge="start"
                onClick={toggleDrawer(true)}
                sx={{ mr: 2 }}
            >
                <MenuIcon />
            </IconButton>
            <Drawer anchor="left" open={drawerOpen} onClose={toggleDrawer(false)}>
                <Box
                    sx={{ width: 250 }}
                    role="presentation"
                    onKeyDown={toggleDrawer(false)}
                >
                    <List component="nav">
                        <Link href="/" passHref legacyBehavior>
                            <ListItemButton component="a" onClick={toggleDrawer(false)}>
                                <ListItemText primary="Home" />
                            </ListItemButton>
                        </Link>
                        <Link href="/plutus" passHref legacyBehavior>
                            <ListItemButton component="a" onClick={toggleDrawer(false)}>
                                <ListItemText primary="Plutus" />
                            </ListItemButton>
                        </Link>
                        <ListItemButton onClick={handleAphroditeClick}>
                            <ListItemText primary="Aphrodite" />
                            {aphroditeOpen ? <ExpandLess /> : <ExpandMore />}
                        </ListItemButton>
                        <Collapse in={aphroditeOpen} timeout="auto" unmountOnExit>
                            <List component="div" disablePadding>
                                <Link href="/aphrodite/conversation" passHref legacyBehavior>
                                    <ListItemButton
                                        component="a"
                                        onClick={toggleDrawer(false)}
                                        sx={{ pl: 4 }}
                                    >
                                        <ListItemText primary="Create Conversation" />
                                    </ListItemButton>
                                </Link>
                                <Link href="/aphrodite/characters" passHref legacyBehavior>
                                    <ListItemButton
                                        component="a"
                                        onClick={toggleDrawer(false)}
                                        sx={{ pl: 4 }}
                                    >
                                        <ListItemText primary="Manage Character" />
                                    </ListItemButton>
                                </Link>
                            </List>
                        </Collapse>
                    </List>
                </Box>
            </Drawer>
        </>
    );
};
