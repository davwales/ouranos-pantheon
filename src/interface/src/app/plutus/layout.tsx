import Box from "@/app/components/core/layout/box";
import { OuranosProvider } from "@/app/services/gql_client";

export default function PlutusLayout({ children }: React.PropsWithChildren) {
    return (
        <Box styling={{ m: "medium" }}>
            <OuranosProvider>
                {children}
            </OuranosProvider>
        </Box>
    )
}
