"use client";

import ExpandMoreIcon from '@/app/components/core/icons/expand_more_icon';
import Button from '@/app/components/core/inputs/button';
import Box from '@/app/components/core/layout/box';
import Menu from '@/app/components/core/navigation/menu';
import MenuItem from '@/app/components/core/navigation/menu_item';
import Link from 'next/link';
import React, { useState } from 'react';

export default function DesktopNavigation() {
    const [anchorEl, setAnchorEl] = useState<HTMLElement | undefined>();

    const handleMenuOpen = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleMenuClose = () => {
        setAnchorEl(undefined);
    };

    return (
        <Box styling={{ display: "flex", alignItems: "center" }}>
            <Link href="/" passHref legacyBehavior>
                <Button color="inherit" component="a">
                    Home
                </Button>
            </Link>
            <Button
                color="inherit"
                onClick={handleMenuOpen}
                endIcon={<ExpandMoreIcon />}
            >
                Hermes
            </Button>
            <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={handleMenuClose}
            >
                <Link href="/hermes/conversation" passHref legacyBehavior>
                    <MenuItem onClick={handleMenuClose} component="a">
                        Create Conversation
                    </MenuItem>
                </Link>
                <Link href="/hermes/characters" passHref legacyBehavior>
                    <MenuItem onClick={handleMenuClose} component="a">
                        Manage Characters
                    </MenuItem>
                </Link>
            </Menu>
            <Link href="/plutus" passHref legacyBehavior>
                <Button color="inherit" component="a">
                    Plutus
                </Button>
            </Link>
        </Box>
    );
};
