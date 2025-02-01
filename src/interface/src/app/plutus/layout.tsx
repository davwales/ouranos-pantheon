import { Box } from '@mui/material';
import { OuranosProvider } from '../services/gql_client';

export default function PlutusLayout({ children }: React.PropsWithChildren) {
    return (
        <Box sx={{ m: "1rem" }}>
            <OuranosProvider>
                {children}
            </OuranosProvider>
        </Box>
    )
}
