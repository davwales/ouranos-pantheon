import Typography from "@/app/components/core/data-display/typography";
import Grid from "@/app/components/core/layout/grid";
import CardContent from "@/app/components/core/surfaces/card_content";
import LinkCard from "@/app/components/surfaces/link_card";

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
            <Grid container spacing={2}>
                {modules.map((m, index) => (
                    <Grid key={index} size={{ sm: 12, lg: 4 }}>
                        <LinkCard href={m.href}>
                            <CardContent>
                                <Typography variant="h4" styling={{ textAlign: "center" }}>
                                    {m.title}
                                </Typography>
                            </CardContent>
                        </LinkCard>
                    </Grid>
                ))}
            </Grid>
        </>
    );
}