import { OuranosProvider } from '../services/gql_client';

export default function PlutusLayout({ children }: React.PropsWithChildren) {
    return (
        <>
            <OuranosProvider>
                {children}
            </OuranosProvider>
        </>
    )
}
