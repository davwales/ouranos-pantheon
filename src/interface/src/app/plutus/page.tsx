import Typography from "@/app/components/core/data-display/typography";
import Grid from "@/app/components/core/layout/grid";
import CardContent from "@/app/components/core/surfaces/card_content";
import LinkCard from "@/app/components/surfaces/link_card";

export default function Plutus() {
    const plutusModule = (name: string, href: string) => (
        <Grid key={name} size={{ sm: 12, md: 6, lg: 4, xl: 2 }}>
            <LinkCard href={href}>
                <CardContent>
                    <Typography variant="h4" styling={{ textAlign: "center" }}>
                        {name}
                    </Typography>
                </CardContent>
            </LinkCard>
        </Grid>
    );

    const explorerModule = plutusModule('Explorer', '/plutus/explorer');

    return (
        <Grid container spacing={2}>
            {explorerModule}
        </Grid>
    );
}