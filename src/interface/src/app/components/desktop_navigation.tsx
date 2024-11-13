// components/DesktopNavigation.tsx
"use client";

import React, { useState } from 'react';
import { Box, Button, Menu, MenuItem } from '@mui/material';
import Link from 'next/link';
import { ExpandMore } from '@mui/icons-material';

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
                Aphrodite
            </Button>
            <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={handleMenuClose}
                MenuListProps={{
                    'aria-labelledby': 'aphrodite-button',
                }}
            >
                <Link href="/aphrodite/conversation" passHref legacyBehavior>
                    <MenuItem onClick={handleMenuClose} component="a">
                        Create Conversation
                    </MenuItem>
                </Link>
                <Link href="/aphrodite/characters" passHref legacyBehavior>
                    <MenuItem onClick={handleMenuClose} component="a">
                        Manage Character
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
