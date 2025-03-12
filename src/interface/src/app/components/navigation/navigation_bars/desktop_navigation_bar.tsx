"use client";

import ExpandLessIcon from '@/app/components/core/icons/expand_less_icon';
import ExpandMoreIcon from '@/app/components/core/icons/expand_more_icon';
import Button from '@/app/components/core/inputs/button';
import Box from '@/app/components/core/layout/box';
import Menu from '@/app/components/core/navigation/menu';
import MenuItem from '@/app/components/core/navigation/menu_item';
import { NavigationBarItem } from '@/app/components/navigation/navigation_bars/navigation_bar_items';
import Link from 'next/link';
import { Fragment, useState } from 'react';

interface DesktopNavigationBarProps {
    items: NavigationBarItem[];
}

export default function DesktopNavigationBar(props: DesktopNavigationBarProps) {
    const [anchorElements, setAnchorElements] = useState<Record<string, HTMLElement | undefined>>({});

    const handleMenuOpen = (label: string, element: HTMLElement) => {
        setAnchorElements(prev => ({
            ...prev,
            [label]: element
        }));
    };

    const handleMenuClose = (label: string) => {
        setAnchorElements(prev => ({
            ...prev,
            [label]: undefined
        }));
    };

    const singleItem = (item: NavigationBarItem, key: number) => (
        <Link key={key} href={item.options[0].href || "#"} passHref legacyBehavior>
            <Button color="inherit" component="a">
                {item.label}
            </Button>
        </Link>
    );

    const multiItem = (item: NavigationBarItem, key: number) => {
        const anchorEl = anchorElements[item.label];

        return (
            <Fragment key={key}>
                <Button
                    color="inherit"
                    onClick={(e) => handleMenuOpen(item.label, e.currentTarget)}
                    endIcon={!anchorEl ? <ExpandMoreIcon /> : <ExpandLessIcon />}
                >
                    {item.label}
                </Button>
                <Menu
                    anchorEl={anchorEl}
                    open={Boolean(anchorEl)}
                    onClose={() => handleMenuClose(item.label)}
                >
                    {item.options?.map((option, optionIndex) => (
                        <Link
                            key={optionIndex}
                            href={option.href}
                            passHref
                            legacyBehavior
                        >
                            <MenuItem
                                onClick={() => handleMenuClose(item.label)}
                                component="a"
                            >
                                {option.label}
                            </MenuItem>
                        </Link>
                    ))}
                </Menu>
            </Fragment>
        );
    };

    const renderNavigationItem = (item: NavigationBarItem, index: number) => {
        const isSingleItem = !item.options || item.options.length <= 1;
        return isSingleItem ? singleItem(item, index) : multiItem(item, index);
    };

    return (
        <Box styling={{ display: "flex", alignItems: "center" }}>
            {props.items.map((item, index) => renderNavigationItem(item, index))}
        </Box>
    );
}
