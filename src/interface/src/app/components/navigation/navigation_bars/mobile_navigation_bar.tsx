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
import { NavigationBarItem } from '@/app/components/navigation/navigation_bars/navigation_bar_items';
import Link from 'next/link';
import React, { Fragment, useState } from 'react';

interface MobileNavigationBarProps {
    items: NavigationBarItem[];
}

export default function MobileNavigationBar(props: MobileNavigationBarProps) {
    const [drawerOpen, setDrawerOpen] = useState(false);
    const [openSections, setOpenSections] = useState<Record<string, boolean>>({});

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

    const toggleSection = (label: string) => {
        setOpenSections(prev => ({
            ...prev,
            [label]: !prev[label]
        }));
    };

    const singleItem = (item: NavigationBarItem, key: number) => (
        <Link key={key} href={item.options[0].href || "#"} passHref legacyBehavior>
            <ListItemButton component="a" onClick={toggleDrawer(false)}>
                <ListItemText primary={item.label} />
            </ListItemButton>
        </Link>
    );

    const multiItem = (item: NavigationBarItem, key: number) => {
        const isOpen = !!openSections[item.label];

        return (
            <Fragment key={key}>
                <ListItemButton onClick={() => toggleSection(item.label)}>
                    <ListItemText primary={item.label} />
                    {isOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                </ListItemButton>
                <Collapse in={isOpen}>
                    <List disablePadding>
                        {item.options?.map((option, optionIndex) => (
                            <Link
                                key={optionIndex}
                                href={option.href}
                                passHref
                                legacyBehavior
                            >
                                <ListItemButton
                                    component="a"
                                    onClick={toggleDrawer(false)}
                                    styling={{ pl: 'large' }}
                                >
                                    <ListItemText primary={option.label} />
                                </ListItemButton>
                            </Link>
                        ))}
                    </List>
                </Collapse>
            </Fragment>
        );
    };

    const renderNavigationItem = (item: NavigationBarItem, index: number) => {
        const isSingleItem = !item.options || item.options.length <= 1;
        return isSingleItem ? singleItem(item, index) : multiItem(item, index);
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
                        {props.items.map((item, index) => renderNavigationItem(item, index))}
                    </List>
                </Box>
            </Drawer>
        </>
    );
}
