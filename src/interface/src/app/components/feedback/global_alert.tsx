"use client";

import Alert from '@/app/components/core/feedback/alert';
import Snackbar from '@/app/components/core/feedback/snackbar';
import Stack from '@/app/components/core/layout/stack';
import { useGlobalState } from '@/app/store/global_state';

export default function GlobalAlert() {
    const { alerts, removeAlert } = useGlobalState();

    return (
        <Stack spacing='xl'>
            {alerts.map((alert) => (
                <Snackbar
                    key={alert.id}
                    open={true}
                    anchorOrigin={{ vertical: "top", horizontal: "right" }}
                    onClose={() => removeAlert(alert.id)}
                >
                    <Alert
                        onClose={() => removeAlert(alert.id)}
                        severity={alert.type}
                        styling={{ width: '100%' }}
                    >
                        {alert.message}
                    </Alert>
                </Snackbar>
            ))}
        </Stack>
    );
};
