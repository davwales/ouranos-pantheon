import { AppRouterCacheProvider as MuiAppRouterCacheProvider } from '@mui/material-nextjs/v14-appRouter';

interface AppRouterCacheProviderProps {
    children: React.ReactNode;
}

export default function AppRouterCacheProvider(props: AppRouterCacheProviderProps) {
    return (
        <MuiAppRouterCacheProvider>
            {props.children}
        </MuiAppRouterCacheProvider>
    );
}