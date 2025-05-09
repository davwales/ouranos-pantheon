import { OuranosProvider } from "@/app/services/gql_client";

export default function PlutusLayout({ children }: React.PropsWithChildren) {
    return (
        <div className="m-4">
            <OuranosProvider>
                {children}
            </OuranosProvider>
        </div>
    )
}
