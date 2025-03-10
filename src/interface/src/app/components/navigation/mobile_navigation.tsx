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
    const [hermesOpen, setHermesOpen] = useState(false);

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
        setHermesOpen(!hermesOpen);
    };

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
                        <ListItemButton onClick={handleHermesClick}>
                            <ListItemText primary="Hermes" />
                            {hermesOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                        </ListItemButton>
                        <Collapse in={hermesOpen}>
                            <List disablePadding>
                                <Link href="/hermes/conversation" passHref legacyBehavior>
                                    <ListItemButton
                                        component="a"
                                        onClick={toggleDrawer(false)}
                                        styling={{ pl: 'large' }}
                                    >
                                        <ListItemText primary="Create Conversation" />
                                    </ListItemButton>
                                </Link>
                                <Link href="/hermes/characters" passHref legacyBehavior>
                                    <ListItemButton
                                        component="a"
                                        onClick={toggleDrawer(false)}
                                        styling={{ pl: 'large' }}
                                    >
                                        <ListItemText primary="Manage Characters" />
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
