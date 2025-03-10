import { OuranosProvider } from "@/app/services/gql_client";


export default function HermesLayout({ children }: React.PropsWithChildren) {
    return (
        <>
            <OuranosProvider>
                {children}
            </OuranosProvider>
        </>
    )
}
