import {
    Drawer,
    ListItemIcon,
    ListItemText,
    Menu,
    MenuItem,
    Typography,
    useMediaQuery,
    useTheme
} from '@mui/material';
import React from 'react';

interface MenuAction {
    label: string;
    icon: React.ReactNode;
    onClick: () => void;
}

interface ResponsiveMenuProps {
    anchorEl: HTMLElement | null;
    onClose: () => void;
    actions: MenuAction[];
    title?: string;
}

export default function ResponsiveMenu(props: ResponsiveMenuProps) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

    const renderMenuItems = () => (
        <>
            {props.actions.map((action, index) => (
                <MenuItem key={index} onClick={() => {
                    action.onClick();
                    props.onClose();
                }}>
                    <ListItemIcon>{action.icon}</ListItemIcon>
                    <ListItemText>{action.label}</ListItemText>
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
                PaperProps={{
                    sx: {
                        borderTopLeftRadius: 8,
                        borderTopRightRadius: 8,
                        padding: 2
                    }
                }}
            >
                {props.title && (
                    <Typography variant="h6" sx={{ pb: 2, textAlign: 'center' }}>
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