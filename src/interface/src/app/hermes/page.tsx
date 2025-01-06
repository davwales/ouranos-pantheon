import { CardContent, Grid2, Typography } from "@mui/material";
import LinkCard from "../components/link_card";

export default function Hermes() {
    const modules = [
        {
            title: "Create Conversation",
            href: "/hermes/conversation"
        },
        {
            title: "Manage Characters",
            href: "/hermes/characters"
        }
    ];

    return (
        <>
            <Grid2 container spacing={2}>
                {modules.map((m, index) => (
                    <Grid2 key={index} size={{ sm: 12, lg: 4 }}>
                        <LinkCard href={m.href}>
                            <CardContent>
                                <Typography variant="h4" sx={{ textAlign: "center" }}>
                                    {m.title}
                                </Typography>
                            </CardContent>
                        </LinkCard>
                    </Grid2>
                ))}
            </Grid2>
        </>
    );
}