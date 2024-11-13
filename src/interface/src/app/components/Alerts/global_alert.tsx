// components/Alerts/GlobalAlert.tsx
"use client";

import React from 'react';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import { useGlobalState } from '@/app/store/global_state';
import { Stack } from '@mui/material';

export default function GlobalAlert() {
    const { alerts, removeAlert } = useGlobalState();

    return (
        <Stack spacing={7}>
            {alerts.map((alert) => (
                <Snackbar
                    key={alert.id}
                    open={true}
                    anchorOrigin={{ vertical: "top", horizontal: "right" }}
                    autoHideDuration={6000}
                    onClose={() => removeAlert(alert.id)}
                >
                    <Alert onClose={() => removeAlert(alert.id)} severity={alert.type} sx={{ width: '100%' }}>
                        {alert.message}
                    </Alert>
                </Snackbar>
            ))}
        </Stack>
    );
};
