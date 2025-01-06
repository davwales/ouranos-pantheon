import { OuranosProvider } from '../services/gql_client';

export default function AphroditeLayout({ children }: React.PropsWithChildren) {
    return (
        <>
            <OuranosProvider>
                {children}
            </OuranosProvider>
        </>
    )
}
