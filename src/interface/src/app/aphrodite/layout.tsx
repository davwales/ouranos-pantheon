import { TalosProvider } from '../services/gql_client';

export default function AphroditeLayout({ children }: React.PropsWithChildren) {
    return (
        <>
            <TalosProvider>
                {children}
            </TalosProvider>
        </>
    )
}
