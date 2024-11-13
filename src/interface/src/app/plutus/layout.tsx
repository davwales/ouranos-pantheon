import { TalosProvider } from '../services/gql_client';

export default function PlutusLayout({ children }: React.PropsWithChildren) {
    return (
        <>
            <TalosProvider>
                {children}
            </TalosProvider>
        </>
    )
}
