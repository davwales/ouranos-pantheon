// components/DesktopNavigation.tsx
"use client";

import { ExpandMore } from '@mui/icons-material';
import { Box, Button, Menu, MenuItem } from '@mui/material';
import Link from 'next/link';
import React, { useState } from 'react';

export default function DesktopNavigation() {
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

    const handleMenuOpen = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleMenuClose = () => {
        setAnchorEl(null);
    };

    return (
        <Box display="flex" alignItems="center">
            <Link href="/" passHref legacyBehavior>
                <Button color="inherit" component="a">
                    Home
                </Button>
            </Link>
            <Button
                color="inherit"
                onClick={handleMenuOpen}
                endIcon={<ExpandMore />}
            >
                Hermes
            </Button>
            <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={handleMenuClose}
                MenuListProps={{
                    'aria-labelledby': 'hermes-button',
                }}
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
