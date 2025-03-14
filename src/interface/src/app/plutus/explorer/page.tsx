import Typography from "@/app/components/core/data-display/typography";
import Box from "@/app/components/core/layout/box";
import MarketSelectionView from "@/app/plutus/views/market_selection_view";

export default function PlutusExplorer() {
    return (
        <Box>
            <Typography variant="h3" styling={{ mb: 'medium' }}>Explorer</Typography>
            <MarketSelectionView href="/plutus/explorer" />
        </Box>
    );
}