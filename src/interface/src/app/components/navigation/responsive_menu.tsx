import ListItemIcon from '@/app/components/core/data-display/list_item_icon';
import ListItemText from '@/app/components/core/data-display/list_item_text';
import Typography from '@/app/components/core/data-display/typography';
import Drawer from '@/app/components/core/navigation/drawer';
import Menu from '@/app/components/core/navigation/menu';
import MenuItem from '@/app/components/core/navigation/menu_item';
import { useMobile } from '@/app/components/core/utils/breakpoints';
import React from 'react';

interface MenuAction {
    label: string;
    icon: React.ReactNode;
    onClick: () => void;
}

interface ResponsiveMenuProps {
    onClose: () => void;
    actions: MenuAction[];
    anchorEl?: HTMLElement;
    title?: string;
}

export default function ResponsiveMenu(props: ResponsiveMenuProps) {
    const isMobile = useMobile();

    const renderMenuItems = () => (
        <>
            {props.actions.map((action, index) => (
                <MenuItem key={index} onClick={() => {
                    action.onClick();
                    props.onClose();
                }}>
                    <ListItemIcon>{action.icon}</ListItemIcon>
                    <ListItemText primary={action.label} />
                </MenuItem>
            ))}
        </>
    );

    if (isMobile) {
        return (
            <Drawer
                anchor="bottom"
                open={Boolean(props.anchorEl)}
                onClose={props.onClose}
                paperStyling={{
                    borderTopLeftRadius: 8,
                    borderTopRightRadius: 8,
                    p: 'large'
                }}
            >
                {props.title && (
                    <Typography variant="h6" styling={{ pb: 'large', textAlign: 'center' }}>
                        {props.title}
                    </Typography>
                )}
                {renderMenuItems()}
            </Drawer>
        );
    }

    return (
        <Menu
            anchorEl={props.anchorEl}
            open={Boolean(props.anchorEl)}
            onClose={props.onClose}
        >
            {renderMenuItems()}
        </Menu>
    );
};